using System;
using Assets.Astrolabe.Scripts.Operations;
using Assets.Astrolabe.Scripts.Tools;
using Astrolabe.Twinkle;
using UnityEngine;
using UnityEngine.Video;

namespace Assets.Astrolabe.Scripts.Visuals
{
	public class VisualMediaPlayerController : IVisualMediaPlayerController
	{
		public VisualMediaPlayerController()
		{
		}

		public bool AutoPlay
		{
			get => _autoPlay;

			set
			{
				if (_autoPlay != value)
				{
					_autoPlay = value;

					if (_videoPlayer != null)
					{
						_videoPlayer.playOnAwake = value;
					}
				}
			}
		}

		private bool _autoPlay = true;

		public bool IsLooping
		{
			get => _isLooping;

			set
			{
				_isLooping = value;

				if (_videoPlayer != null)
				{
					_videoPlayer.isLooping = value;
				}
			}
		}

		private bool _isLooping = false;

		public Uri Source
		{
			get => _source;

			set
			{
				if (_source != value)
				{
					_source = value;

					VideoPlayer.Stop();
					VideoPlayer.playOnAwake = AutoPlay;
					VideoPlayer.isLooping = IsLooping;

					// this.VideoPlayer.url ne supporte pas le null
					if (value != null)
					{
						State = VisualMediaPlayerStates.Opening;

						// this.VideoPlayer.url ne supporte que http/https ou file:// comme scheme

						if (value.IsHttpScheme() == true)
						{
							VideoPlayer.url = value.ToString();
						}
						else
						{
							var fullfilename = UriInformation.ParseToRead(_source).GetPath();

							// en mode file, il ne supporte pas les %20 pour les espaces :/
							fullfilename = fullfilename.Replace("%20", " ");

							var uriFile = "file://" + fullfilename;

							// nettoyage de la texture si elle existe car le temps de chargement des vidéos peut être long et la texture garde la dernière image affichée
							if (Texture != null)
							{
								Texture.Clear();
							}

							VideoPlayer.url = uriFile?.ToString();
						}

						VideoPlayer.Prepare();
					}
				}
			}
		}

		private Uri _source;

		public void Clear()
		{
			if (Texture != null)
			{
				Texture.Clear();
			}
		}

		public AudioSource GetAudioTrackSource(int trackIndex)
		{
			return _videoPlayer.GetTargetAudioSource((ushort)trackIndex);
		}

		public int AudioTrackCount => _videoPlayer.controlledAudioTrackCount;

		/// <summary>
		/// LA préparation de la vidéo est terminé
		/// </summary>
		/// <param name="source"></param>
		private void VideoPlayer_prepareCompleted(VideoPlayer source)
		{
			VideoPlayer.renderMode = VideoRenderMode.RenderTexture;
			VideoPlayer.aspectRatio = VideoAspectRatio.Stretch;

			Duration = TimeSpan.FromSeconds(VideoPlayer.length);

			if (Texture == null || Texture.width != source.texture.width || Texture.height != source.texture.height)
			{
				// ici on applique le RenderTexture
				var texture = new RenderTexture(source.texture.width, source.texture.height, 0); // 0 = no ZBuffer

				texture.name = "AstrolabeVideoPlayerTexture";

				Texture = texture;
				VideoPlayer.targetTexture = texture;

				TextureCreated?.Invoke(this, EventArgs.Empty);
			}

			ReadyToPlay?.Invoke(this, EventArgs.Empty);

			State = VisualMediaPlayerStates.Opened;
		}

		// La texture de la vidéo a été crée (si première fois ou si la source à un taille différnte de l'ancienne)
		public event EventHandler TextureCreated;

		// La vidéo est prête à être affichée (à chaque changement de source)
		public event EventHandler ReadyToPlay;

		public RenderTexture Texture { get; private set; }

		public VideoPlayer VideoPlayer
		{
			get
			{
				if (_videoPlayer == null)
				{
					var go = new GameObject();
					go.name = "GameObjectVideoPlayer";

					var vp = go.AddComponent<VideoPlayer>();

					vp.prepareCompleted += VideoPlayer_prepareCompleted;
					vp.seekCompleted += VideoPlayer_seekCompleted;
					vp.loopPointReached += VideoPlayer_loopPointReached;

					// Update pour gérer les états du VideoPlayer
					TwinkleApplication.Instance.Rendering += Instance_Rendering;

					_videoPlayer = vp;

					// Initialisation des proriétés du VideoPlayer

					// Un volume a deja été demandé par le user
					if (_isVolumeChangeBeforeVideoPlayerInitialized == true)
					{
						Volume = _volume;
					}
					else
					{
						// On fixe la valeur par defaut du volume par le player
						Volume = _videoPlayer.GetDirectAudioVolume(0);
					}

					IsMute = _isMute;

					// va ajouter VisualMediPlayer dans les elements à disposer automatiquement lors d'un changement de la chaine TML
					// et fixe le GameObject comme enfant de Astrolabe/Void
					AstrolabeManager.Instance.Engine.VoidObject.AddToVoid(this, go);
				}

				return _videoPlayer;
			}
		}

		/// <summary>
		/// Changement de time après appel de Seek
		/// </summary>
		/// <param name="source"></param>
		private void VideoPlayer_seekCompleted(VideoPlayer source)
		{
			SeekCompleted?.Invoke(this, new EventSeekPositionArgs(_seekPosition));
		}

		public event EventHandler SeekCompleted;

		/// <summary>
		/// Fin de la vidéo
		/// </summary>
		/// <param name="source"></param>
		private void VideoPlayer_loopPointReached(VideoPlayer source)
		{
			VideoEnded?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Update pour gérer les états du VideoPlayer
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Instance_Rendering(object sender, EventArgs e)
		{
			if (_videoPlayer != null)
			{
				//bool isBuffering = this.UpdateIsBuffering();
				var isBuffering = false;

				// Quand on appuie sur Stop // isPrepared passe à false
				if (_videoPlayer.isPrepared == false)
				{
					// on teste si l'etat est en train de jouer ou pas
					if (State == VisualMediaPlayerStates.Playing || State == VisualMediaPlayerStates.Paused)
					{
						State = VisualMediaPlayerStates.Stopped;
					}
				}
				else if (_videoPlayer.isPaused)
				{
					State = VisualMediaPlayerStates.Paused;
				}
				else if (_videoPlayer.isPlaying)
				{
					// detextion du Buffering
					if (isBuffering == true)
					{
						State = VisualMediaPlayerStates.Buffering;
					}
					else
					{
						State = VisualMediaPlayerStates.Playing;
					}
				}

				// Position de la vidéo
				Position = TimeSpan.FromSeconds(_videoPlayer.time);
			}
		}

		private VideoPlayer _videoPlayer = null;

		public TimeSpan Duration { get; private set; }

		public TimeSpan Position
		{
			get => _position;

			private set
			{
				if (_position != value)
				{
					_position = value;
					PositionChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		private TimeSpan _position;

		public event EventHandler PositionChanged;

		public bool CanSeek
		{
			get
			{
				if (VideoPlayer == null)
				{
					return false;
				}

				return VideoPlayer.canSetTime;
			}
		}

		/// <summary>
		/// Recherche
		/// </summary>
		/// <param name="position"></param>
		public void Seek(TimeSpan position)
		{
			if (VideoPlayer != null)
			{
				VideoPlayer.time = position.TotalSeconds;
				_seekPosition = position;
				// peut être changement de l'état Buffering ici ???
			}
		}

		private int _bufferingHit = 0;
		private double _lastTimePlayed;
		private TimeSpan _seekPosition;

		/// <summary>
		/// Permet de determiner si l'on bufferise ou non
		/// </summary>
		/// <returns></returns>
		private bool UpdateIsBuffering()
		{
			if (_videoPlayer == null)
			{
				return false;
			}

			var isBuffering = false;

			//when the video is playing, check each time that the video image get update based in the video's frame rate
			if (_videoPlayer.isPlaying == true)
			{
				if (_lastTimePlayed == _videoPlayer.time)
				{
					// après 10 frames (60ms * 10 = 600ms)  on est sur que l'on bufferise
					if (_bufferingHit > 10)
					{
						isBuffering = true;
					}
					else
					{
						_bufferingHit++;
					}
				}

				_lastTimePlayed = _videoPlayer.time;
			}
			else
			{
				_lastTimePlayed = -1;
				_bufferingHit = 0;
			}

			return isBuffering;
		}

		public bool IsDisposed { get; private set; }

		public void Dispose()
		{
			if (IsDisposed == false)
			{
				if (VideoPlayer != null)
				{
					IsDisposed = true;

					VideoPlayer.prepareCompleted -= VideoPlayer_prepareCompleted;
					VideoPlayer.loopPointReached -= VideoPlayer_loopPointReached;
					VideoPlayer.seekCompleted -= VideoPlayer_seekCompleted;

					// Update pour gérer les états du VideoPlayer
					TwinkleApplication.Instance.Rendering -= Instance_Rendering;

					Stop();

					UnityEngine.Object.Destroy(_videoPlayer.gameObject);

					if (Texture != null)
					{
						UnityEngine.Object.Destroy(Texture);
						Texture = null;
					}

					AstrolabeManager.Instance.Engine.VoidObject.DisposeFromVoid(this);
				}
			}
		}

		public void Pause()
		{
			if (Source != null)
			{
				VideoPlayer.Pause();
			}
		}

		public void Play()
		{
			if (Source != null)
			{
				VideoPlayer.Play();
			}
		}

		public void Stop()
		{
			VideoPlayer.Stop();
			Texture.Clear();
			VideoPlayer.Prepare();
		}

		public VisualMediaPlayerStates State
		{
			get => _state;

			set
			{
				if (_state != value)
				{
					_state = value;
					StateChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		private VisualMediaPlayerStates _state = VisualMediaPlayerStates.Stopped;

		public bool IsMute
		{
			get => _isMute;

			set
			{
				if (_isMute != value)
				{
					_isMute = value;

					if (_videoPlayer != null)
					{
						_videoPlayer.SetDirectAudioMute(0, value);
					}

					IsMuteChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		private bool _isMute = false;

		/// <summary>
		/// Volume de 0 à 1
		/// </summary>

		public float Volume
		{
			get => _volume;

			set
			{
				if (_volume != value)
				{
					_volume = value;

					if (_videoPlayer != null)
					{
						_isVolumeChangeBeforeVideoPlayerInitialized = false;
						_videoPlayer.SetDirectAudioVolume(0, _volume);
					}
					else
					{
						_isVolumeChangeBeforeVideoPlayerInitialized = true;
					}

					VolumeChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		private float _volume = 0.5f;
		private bool _isVolumeChangeBeforeVideoPlayerInitialized = false;

		/// <summary>
		/// Changement dans l'etat détécté
		/// </summary>
		public event EventHandler StateChanged;

		/// <summary>
		/// Fin d'une vidéo
		/// </summary>
		public event EventHandler VideoEnded;

		public event EventHandler VolumeChanged;

		public event EventHandler IsMuteChanged;
	}

	public class EventSeekPositionArgs : EventArgs
	{
		public EventSeekPositionArgs(TimeSpan position)
		{
			Position = position;
		}

		public TimeSpan Position { get; private set; }
	}
}