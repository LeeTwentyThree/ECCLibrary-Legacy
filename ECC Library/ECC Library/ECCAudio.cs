using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ECCLibrary.Internal;

namespace ECCLibrary
{
    public static class ECCAudio
    {
        public static Dictionary<string, AudioClip> allClips;

        public static void RegisterClips(AssetBundle fromAssetBundle)
        {
            if(fromAssetBundle == null)
            {
                ECCLog.AddMessage("Asset bundle is 'null'.");
            }
            if (allClips == null) allClips = new Dictionary<string, AudioClip>();
            AudioClip[] loadedClips = fromAssetBundle.LoadAllAssets<AudioClip>();
            if(loadedClips == null || loadedClips.Length == 0)
            {
                ECCLog.AddMessage("RegisterClips was called but the asset bundle has no clips to load.");
            }
            foreach(AudioClip clip in loadedClips)
            {
                if (allClips.ContainsKey(clip.name))
                {
                    ECCLog.AddMessage("ModAudio already contains an AudioClip by name {0}. This clip is being ignored.", clip.name);
                }
                else
                {
                    allClips.Add(clip.name, clip);
                }
            }
        }

        public static AudioClipPool CreateClipPool(string startingLetters)
        {
            if (allClips == null || allClips.Count == 0)
            {
                ECCLog.AddMessage("Cannot load audio clips; no clips are registered.");
                return null;
            }
            if (string.IsNullOrEmpty(startingLetters))
            {
                ECCLog.AddMessage("Notice: creating clip pool from empty string");
            }
            List<AudioClip> clips = new List<AudioClip>();
            string[] allAudioKeys = allClips.Keys.ToArray();
            for (int i = 0; i < allClips.Count; i++)
            {
                if (allAudioKeys[i].StartsWith(startingLetters))
                {
                    clips.Add(allClips.Values.ElementAt(i));
                }
            }
            if(clips.Count == 0)
            {
                ECCLog.AddMessage("No registered audio clips starting with prefix {0}.", startingLetters);
            }
            return new AudioClipPool(clips.ToArray());
        }

        public static AudioClip LoadAudioClip(string byName)
        {
            if(allClips == null || allClips.Count == 0)
            {
                ECCLog.AddMessage("Cannot load an audio clip; no clips are registered.");
                return null;
            }
            if(allClips.TryGetValue(byName, out var outClip))
            {
                return outClip;
            }
            else
            {
                ECCLog.AddMessage("No clip found by name {0}.", byName);
                return null;
            }
        }

        public class AudioClipPool
        {
            public AudioClip[] clips;

            public AudioClipPool(AudioClip[] clips)
            {
                this.clips = clips;
            }

            public AudioClip GetRandomClip()
            {
                return clips[UnityEngine.Random.Range(0, clips.Length)];
            }
        }
    }
}
