﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ECCLibrary.Internal;
using QModManager.Utility;

namespace ECCLibrary
{
    /// <summary>
    /// Class related to audio of ECC. Used by several internal and publlic functions.
    /// </summary>
    public static class ECCAudio
    {
        /// <summary>
        /// All UnityEngine.AudioClips registered in ECCLibrary.
        /// </summary>
        public static Dictionary<string, AudioClip> allClips;

        /// <summary>
        /// Register ALL audio clips from the given asset bundle.
        /// </summary>
        /// <param name="assetBundle"></param>
        public static void RegisterClips(AssetBundle assetBundle)
        {
            if(assetBundle == null)
            {
                ECCLog.AddMessage("Asset bundle is 'null'.");
            }
            if (allClips == null) allClips = new Dictionary<string, AudioClip>();
            AudioClip[] loadedClips = assetBundle.LoadAllAssets<AudioClip>();
            foreach(AudioClip clip in loadedClips)
            {
                if (allClips.ContainsKey(clip.name))
                {
                    QModManager.API.QModServices.Main.AddCriticalMessage(string.Format("ECC: ModAudio already contains an AudioClip by name {0}. This clip is being ignored.", clip.name));
                }
                else
                {
                    allClips.Add(clip.name, clip);
                }
            }
        }

        /// <summary>
        /// Create a clip pool with all AudioClips that begin with 'startingLetters'.
        /// </summary>
        /// <param name="startingLetters">All clips beginning with this are added to the AudioClipPool.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Load a specific AudioClip by its exact name.
        /// </summary>
        /// <param name="byName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns the master volume for ECC (ranges from 0-1)
        /// </summary>
        /// <returns></returns>
        public static float ECCVolume
        {
            get
            {
                return ECCHelpers.GetECCVolume();
            }
        }

        /// <summary>
        /// An array of AudioClips, which can be generated by ECC. Can be used to select random audio clips.
        /// </summary>
        public class AudioClipPool
        {
            public AudioClip[] clips;

            /// <summary>
            /// Initialize a clip pool. For some reason this method does not work until after the game has been loaded.
            /// </summary>
            /// <param name="clips"></param>
            public AudioClipPool(AudioClip[] clips)
            {
                this.clips = clips;
            }

            /// <summary>
            /// Returns a random clip out of the selection.
            /// </summary>
            /// <returns></returns>
            public AudioClip GetRandomClip()
            {
                if(clips == null | clips.Length == 0)
                {
                    ECCLog.AddMessage("AudioClip.GetRandomClip failed - no clips found.");
                }
                return clips[UnityEngine.Random.Range(0, clips.Length)];
            }
        }
    }
}
