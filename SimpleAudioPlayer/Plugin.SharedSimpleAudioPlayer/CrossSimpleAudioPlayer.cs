using System;

namespace Plugin.SimpleAudioPlayer
{
    /// <summary>
    /// Cross platform SimpleAudioPlayer implemenations
    /// </summary>
#if __ANDROID__
    [Android.Runtime.Preserve(AllMembers = true)]
#endif
    public class CrossSimpleAudioPlayer
    {
        public static Func<ISimpleAudioPlayer> GetSimpleAudioPlayer { get; set; }
        static ISimpleAudioPlayer _Implementation;
        static ISimpleAudioPlayer Implementation
        {
            get
            {
                if (_Implementation == null)
                {
                    _Implementation = CreateSimpleAudioPlayer();
                }
                return _Implementation;
            }
        }

        /// <summary>
        /// Current settings to use
        /// </summary>
        public static ISimpleAudioPlayer Current
        {
            get
            {
                var ret = Implementation;
                if (ret == null)
                {
                    throw NotImplementedInReferenceAssembly();
                }
                return ret;
            }
        }

        ///<Summary>
        /// Create a new SimpleAudioPlayer object
        ///</Summary>
        public static ISimpleAudioPlayer CreateSimpleAudioPlayer()
        {
            return GetSimpleAudioPlayer?.Invoke();
        }

        internal static Exception NotImplementedInReferenceAssembly()
        {
            return new NotImplementedException("This functionality is not implemented in the .NET standard version of this assembly. Reference the NuGet package from your platform-specific (head) application project in order to reference the platform-specific implementation.");
        }
    }
}