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
        public static List<AudioClip> allClips;

        public static void RegisterClips(AssetBundle fromAssetBundle)
        {
            if (allClips == null) allClips = new List<AudioClip>();
            allClips.AddRange(fromAssetBundle.LoadAllAssets<AudioClip>());
        }

        public static AudioClipPool CreateClipPool(string startingLetters)
        {
            if (string.IsNullOrEmpty(startingLetters))
            {
                ECCLog.AddMessage("Notice: creating clip pool from empty string");
            }
            List<AudioClip> clips = new List<AudioClip>();
            for (int i = 0; i < allClips.Count; i++)
            {
                if (allClips[i].name.StartsWith(startingLetters))
                {
                    clips.Add(allClips[i]);
                }
            }
            if(clips.Count == 0)
            {
                ECCLog.AddMessage("No registered audio clips starting with prefix {0}.", startingLetters);
            }
            return new AudioClipPool(clips.ToArray());
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
