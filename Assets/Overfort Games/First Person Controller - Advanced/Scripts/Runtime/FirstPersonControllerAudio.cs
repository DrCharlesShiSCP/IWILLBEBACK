using System;
using UnityEngine;

namespace OverfortGames.FirstPersonController
{
	//Responsible of handling audio for the 'FirstPersonController' class
	public class FirstPersonControllerAudio : MonoBehaviour
	{
		#region Fields
		[Header("References "), Space(5)]

		[SerializeField]
		private FirstPersonController controller;
		[Space(5)]
		[Header("Settings"), Space(5)]

		//Master volume modifier for all the sfx in this script
		[SerializeField, Range(0f, 1f)]
		private float masterVolume = 0.1f;

		//Minimum distance the character has to travel in order to trigger the footstep sfx
		[SerializeField]
		private float footstepMinimumDistance = 0.2f;

		//Apply a random distortion in the volume for the footstep sfx
		[SerializeField]
		private float footstepRandomVolumeDelta = 0.2f;

		[SerializeField]
		private float proneFootstepMinimumDistance = 0.2f;

		//Apply a random distortion in the volume for the footstep sfx
		[SerializeField]
		private float proneFootstepRandomVolumeDelta = 0.2f;

		private float footstepControllerSpeedMinimumThreshold = 0.05f;

		[Space(5)]
		[Header("Audio Clips"), Space(5)]

		[SerializeField]
		private AudioClip[] footstepsClips;

		[SerializeField]
		private AudioClip[] proneFootstepsClips;

		[SerializeField]
		private AudioClip[] jumpsCountIncreasesClips;

		[SerializeField]
		private AudioClip jumpClip;

		[SerializeField]
		private AudioClip landClip;

		[SerializeField]
		private AudioClip beginSlideClip;

		[SerializeField]
		private AudioClip slideClip;

		[SerializeField]
		private AudioClip endSlideClip;

		[SerializeField]
		private AudioClip grapplingClip;

		[SerializeField]
		private AudioClip beginGrapplingClip;

		[SerializeField]
		private AudioClip endGrapplingClip;

		[SerializeField]
		private AudioClip beginGrapplingLineClip;

		[SerializeField]
		private AudioClip endGrapplingLineClip;

		[SerializeField]
		private AudioClip endFailedGrapplingLineClip;

		[SerializeField]
		private AudioClip beginWallRunClip;

		[SerializeField]
		private AudioClip wallRunClip;

		[SerializeField]
		private AudioClip endWallRunClip;

		[SerializeField]
		private AudioClip beginClimbClip;

		[SerializeField]
		private AudioClip endClimbClip;

		private Transform tr;

		//Distance from last footstep sound play
		private float currentFootstepDistance = 0f;

		private AudioSource currentSlideAudioSource;
		private float slideSoundCooldown;
		private float lastTimeSlide;

		private AudioSource currentGrapplingAudioSource;
		private float grapplingSoundCooldown;
		private float lastTimeGrappling;

		private AudioSource currentWallRunAudioSource;
		private float wallRunSoundCooldown;
		private float lastTimeWallRun;

		#endregion

		#region Methods
		private void Start()
		{
			//Caching transform is faster in older versions of Unity
			tr = transform;

			//Plug events
			controller.OnLand += OnLand;
			controller.OnJump += OnJump;
			controller.OnJumpsCountIncrease += OnJumpsCountIncrease;

			controller.OnSlide += OnSlide;
			controller.OnEndSlide += OnEndSlide;
			controller.OnBeginSlide += OnBeginSlide;

			controller.OnGrappling += OnGrappling;
			controller.OnBeginGrappling += OnBeginGrappling;
			controller.OnEndGrappling += OnEndGrappling;

			controller.OnBeginGrapplingLine += OnBeginGrapplingLine;
			controller.OnEndGrapplingLine += OnEndGrapplingLine;
			controller.OnEndFailedGrapplingLine += OnEndFailedGrapplingLine;

			controller.OnBeginWallRun += OnBeginWallRun;
			controller.OnWallRun += OnWallRun;
			controller.OnEndWallRun += OnEndWallRun;

			controller.OnClimbBegin += OnClimbBegin;
			controller.OnClimbEnd += OnClimbEnd;

			//We cache the lenght of the slide clip in order to loop through it while sliding
			if (slideClip)
			{
				slideSoundCooldown = slideClip.length;
				lastTimeSlide = -slideClip.length;
			}

			//We cache the lenght of the grappling clip in order to loop through it while grappling
			if (grapplingClip)
			{
				grapplingSoundCooldown = grapplingClip.length;
				lastTimeGrappling = -grapplingClip.length;
			}

			//We cache the lenght of the grappling clip in order to loop through it while grappling
			if (wallRunClip)
			{
				wallRunSoundCooldown = wallRunClip.length;
				lastTimeWallRun = -wallRunClip.length;
			}
		}

		private void OnDestroy()
		{
			controller.OnLand -= OnLand;
			controller.OnJump -= OnJump;

			//FirstPersonController.OnSlide event gets called every fixed update while the character is sliding
			controller.OnSlide -= OnSlide;

			controller.OnEndSlide -= OnEndSlide;
		}

		//Update;
		void Update()
		{
			PlayFootsteps(footstepsClips, footstepMinimumDistance, footstepRandomVolumeDelta, FirstPersonController.ControllerState.Standing);
			PlayFootsteps(footstepsClips, footstepMinimumDistance, footstepRandomVolumeDelta, FirstPersonController.ControllerState.TacticalSprint);
			PlayFootsteps(footstepsClips, footstepMinimumDistance, footstepRandomVolumeDelta, FirstPersonController.ControllerState.Crouched);
			PlayFootsteps(proneFootstepsClips, proneFootstepMinimumDistance, proneFootstepRandomVolumeDelta, FirstPersonController.ControllerState.Proned);
		}

		private void PlayFootsteps(AudioClip[] clips, float stepMinimumDistance, float randomVolumeDelta, FirstPersonController.ControllerState state)
		{
			//We don't want to play footsteps while not in the target state
			if (controller.currentControllerState != state)
			{
				return;
			}

			Vector3 vel = controller.GetVelocity();

			//Extract the horizotal velocity from velocity
			Vector3 horizontalVelocity = RemoveDotVector(vel, tr.up);

			float currentMovementSpeed = horizontalVelocity.magnitude;
			currentFootstepDistance += Time.deltaTime * currentMovementSpeed;

			//The minumum distance has been reached
			if (currentFootstepDistance > stepMinimumDistance)
			{
				//Play the footstep sound only if the the character is grounded and it's moving faster than the minimum threshold
				if (controller.IsGrounded() && currentMovementSpeed > footstepControllerSpeedMinimumThreshold)
				{
					int randomFoostepClipIndex = UnityEngine.Random.Range(0, clips.Length);
					AudioLibrary.Play2D(clips[randomFoostepClipIndex], masterVolume + masterVolume * UnityEngine.Random.Range(-randomVolumeDelta, randomVolumeDelta));
				}
				currentFootstepDistance = 0f;
			}
		}

		private void OnLand(float landDistance)
		{
			if (landDistance < 0.5f)
				return;

			//Play land audio clip;
			AudioLibrary.Play2D(landClip, masterVolume);
		}

		private void OnJump()
		{
			//Play jump audio clip;
			AudioLibrary.Play2D(jumpClip, masterVolume);
		}

		private void OnJumpsCountIncrease(int jumpsCount)
		{
			if (jumpsCountIncreasesClips.Length < jumpsCount)
				return;

			//Play jump audio clip;
			AudioClip clip = jumpsCountIncreasesClips[jumpsCount - 1];

			if (clip != null)
			{
				AudioLibrary.Play2D(clip, masterVolume);
			}
		}

		private void OnBeginSlide()
		{
			if (beginSlideClip != null)
				AudioLibrary.Play2D(beginSlideClip, masterVolume);
		}

		private void OnEndSlide()
		{
			StopSlideSound();

			if (endSlideClip != null)
				AudioLibrary.Play2D(endSlideClip, masterVolume);

			lastTimeSlide = 0;
		}

		//FirstPersonController.OnSlide event gets called every fixed update while the character is sliding
		private void OnSlide()
		{
			if (IsSlideSoundCooldown() == false)
			{
				currentSlideAudioSource = AudioLibrary.Play2D(slideClip, masterVolume);
				lastTimeSlide = Time.time;
			}
		}

		private void StopSlideSound()
		{
			if (currentSlideAudioSource != null && currentSlideAudioSource.isPlaying)
				currentSlideAudioSource.Stop();

			lastTimeSlide = 0;
		}

		//'slideSoundCooldown' is the lenght of the slide clip, it's done in order to loop through it while sliding
		private bool IsSlideSoundCooldown()
		{
			return Time.time < lastTimeSlide + slideSoundCooldown;
		}

		private void OnBeginGrapplingLine()
		{
			if (beginGrapplingLineClip != null)
			{
				AudioLibrary.Play2D(beginGrapplingLineClip, masterVolume);
			}
		}

		private void OnEndGrapplingLine()
		{
			if (endGrapplingLineClip != null)
			{
				AudioLibrary.Play2D(endGrapplingLineClip, masterVolume);
			}
		}

		private void OnEndFailedGrapplingLine()
		{
			if (endFailedGrapplingLineClip != null)
			{
				AudioLibrary.Play2D(endFailedGrapplingLineClip, masterVolume);
			}
		}

		private void OnBeginGrappling()
		{
			if (beginGrapplingClip != null)
			{
				AudioLibrary.Play2D(beginGrapplingClip, masterVolume);
			}
		}

		private void OnGrappling()
		{
			if (IsGrapplingSoundCooldown() == false)
			{
				currentGrapplingAudioSource = AudioLibrary.Play2D(grapplingClip, masterVolume);
				lastTimeGrappling = Time.time;
			}
		}

		//'grapplingSoundCooldown' is the lenght of the slide clip, it's done in order to loop through it while grappling
		private bool IsGrapplingSoundCooldown()
		{
			return Time.time < lastTimeGrappling + grapplingSoundCooldown;
		}

		private void StopGrapplingSound()
		{
			if (currentGrapplingAudioSource != null && currentGrapplingAudioSource.isPlaying)
				currentGrapplingAudioSource.Stop();

			lastTimeGrappling = 0;
		}

		private void OnEndGrappling()
		{
			StopGrapplingSound();

			if (endGrapplingClip != null)
			{
				AudioLibrary.Play2D(endGrapplingClip, masterVolume);
			}
		}

		private void OnBeginWallRun()
		{
			if (beginWallRunClip != null)
			{
				AudioLibrary.Play2D(beginWallRunClip, masterVolume);
			}
		}

		private void OnEndWallRun()
		{
			StopWallRunSound();

			if (endWallRunClip != null)
			{
				AudioLibrary.Play2D(endWallRunClip, masterVolume);
			}
		}

		private void OnWallRun()
		{
			if (IsWallRunSoundCooldown() == false)
			{
				currentWallRunAudioSource = AudioLibrary.Play2D(wallRunClip, masterVolume);
				lastTimeWallRun = Time.time;
			}
		}

		//'wallRunSoundCooldown' is the lenght of the slide clip, it's done in order to loop through it while wall running
		private bool IsWallRunSoundCooldown()
		{
			return Time.time < lastTimeWallRun + wallRunSoundCooldown;
		}

		private void StopWallRunSound()
		{
			if (currentWallRunAudioSource != null && currentWallRunAudioSource.isPlaying)
				currentWallRunAudioSource.Stop();

			lastTimeWallRun = 0;
		}

		//Remove all parts from a vector that are pointing in the same direction as 'dir'
		public static Vector3 RemoveDotVector(Vector3 vec, Vector3 dir)
		{
			if (dir.sqrMagnitude != 1)
				dir.Normalize();

			float amount = Vector3.Dot(vec, dir);

			vec -= dir * amount;

			return vec;
		}
		private void OnClimbBegin()
		{
			if (beginClimbClip != null)
			{
				AudioLibrary.Play2D(beginClimbClip, masterVolume);
			}
		}

		private void OnClimbEnd()
		{
			if (endClimbClip != null)
			{
				AudioLibrary.Play2D(endClimbClip, masterVolume);
			}
		}

		public void SetMasterVolume(float value)
		{
			masterVolume = value;
		}

		#endregion
	}
}

