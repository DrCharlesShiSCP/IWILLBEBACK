using UnityEngine;

namespace OverfortGames.FirstPersonController
{
    //Handy way of playing clips using ids and static methods
    public class AudioLibrary : MonoBehaviour
    {
        #region Fields

        private const int MAX_POOL_SIZE = 15;

        //Pool of audio sources for 2D usage
        private AudioSource[] sources2D;
        private Transform poolTransform;


        private static float[] volumes;

        private static AudioLibrary _instance;

        public static AudioLibrary Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("Audio Library").AddComponent<AudioLibrary>();
                    _instance.Instantiate2DSources();
                }

                return _instance;
            }
        }

        #endregion

        #region Methods

        private void Instantiate2DSources()
        {
            if (poolTransform == null)
            {
                poolTransform = new GameObject("Pool").transform;
                poolTransform.SetParent(_instance.transform);
            }

            sources2D = new AudioSource[MAX_POOL_SIZE];

            for (int i = 0; i < MAX_POOL_SIZE; i++)
            {
                var go = new GameObject("2D Audio Source " + i);
                go.transform.SetParent(poolTransform);
                sources2D[i] = go.AddComponent<AudioSource>();
            }
        }

        public static AudioSource Play2D(AudioClip clip, float volume = 1)
        {
            if (clip == null)
                return null;

            //Cycle throught the 2D sources until you find one that it's not already playing anything
            foreach (var source2D in Instance.sources2D)
            {
                if (source2D.isPlaying == false)
                {
                    source2D.PlayOneShot(clip, volume);
                    return source2D;
                }
            }

            Debug.LogError("Not enough free 2D sources to play this clip");
            return null;
        }

        public static void MuteAll2D()
        {
            if (volumes == null)
                volumes = new float[Instance.sources2D.Length];

            for (int i = 0; i < Instance.sources2D.Length; i++)
            {
                var source2D = Instance.sources2D[i];

                volumes[i] = source2D.volume;
                source2D.volume = 0;
            }
        }

        public static void UnmuteAll2D()
        {
            if (volumes == null)
                return;

            for (int i = 0; i < Instance.sources2D.Length; i++)
            {
                var source2D = Instance.sources2D[i];

                source2D.volume = volumes[i];
            }
        }

        //Play a clip in the world space position 'point'
        public static AudioSource Play3D(AudioClip clip, Vector3 point)
        {

            //Instantiate a temporarly gameObject just for this play
            GameObject tempGO = new GameObject("TempAudio");

            //Set it to the desired position
            tempGO.transform.position = point;

            //Add the 'AudioSource' component in order to play the audio
            AudioSource aSource = tempGO.AddComponent<AudioSource>();

            //Place the clip and play it
            aSource.clip = clip;
            aSource.Play();

            //Destory the gameObject after the duration of the clip
            Destroy(tempGO, clip.length);

            return aSource;
        }

        #endregion
    }

}