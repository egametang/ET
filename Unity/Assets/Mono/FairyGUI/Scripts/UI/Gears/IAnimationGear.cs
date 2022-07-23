
namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAnimationGear
    {
        /// <summary>
        /// 
        /// </summary>
        bool playing { get; set; }

        /// <summary>
        /// 
        /// </summary>
        int frame { get; set; }

        /// <summary>
        /// 
        /// </summary>
        float timeScale { get; set; }

        /// <summary>
        /// 
        /// </summary>
        bool ignoreEngineTimeScale { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        void Advance(float time);
    }
}
