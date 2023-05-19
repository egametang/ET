// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>
    /// A variant of <see cref="IAnimationClipSource"/> which uses a <see cref="ICollection{T}"/> instead of a
    /// <see cref="List{T}"/> so that it can take a <see cref="HashSet{T}"/> to efficiently avoid adding duplicates.
    /// <see cref="AnimancerUtilities"/> contains various extension methods for this purpose.
    /// </summary>
    /// <remarks>
    /// <see cref="IAnimationClipSource"/> still needs to be the main point of entry for the Animation Window, so this
    /// interface is only used internally.
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/IAnimationClipCollection
    /// 
    public interface IAnimationClipCollection
    {
        /************************************************************************************************************************/

        /// <summary>Adds all the animations associated with this object to the `clips`.</summary>
        void GatherAnimationClips(ICollection<AnimationClip> clips);

        /************************************************************************************************************************/
    }

    /************************************************************************************************************************/

    public static partial class AnimancerUtilities
    {
        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Adds the `clip` to the `clips` if it wasn't there already.
        /// </summary>
        public static void Gather(this ICollection<AnimationClip> clips, AnimationClip clip)
        {
            if (clip != null && !clips.Contains(clip))
                clips.Add(clip);
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Calls <see cref="Gather(ICollection{AnimationClip}, AnimationClip)"/> for each of the `newClips`.
        /// </summary>
        public static void Gather(this ICollection<AnimationClip> clips, IList<AnimationClip> newClips)
        {
            if (newClips == null)
                return;

            for (int i = newClips.Count - 1; i >= 0; i--)
                clips.Gather(newClips[i]);
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Calls <see cref="Gather(ICollection{AnimationClip}, AnimationClip)"/> for each of the `newClips`.
        /// </summary>
        public static void Gather(this ICollection<AnimationClip> clips, IEnumerable<AnimationClip> newClips)
        {
            if (newClips == null)
                return;

            foreach (var clip in newClips)
                clips.Gather(clip);
        }

        /************************************************************************************************************************/

        private static Editor.ConversionCache<Type, MethodInfo> _TypeToGetRootTracks;

        /// <summary>[Animancer Extension]
        /// Calls <see cref="Gather(ICollection{AnimationClip}, AnimationClip)"/> for each clip in the `asset`.
        /// </summary>
        public static void GatherFromAsset(this ICollection<AnimationClip> clips, PlayableAsset asset)
        {
            if (asset == null)
                return;

            // We want to get the tracks out of a TimelineAsset without actually referencing that class directly
            // because it comes from an optional package and Animancer does not need to depend on that package.
            if (_TypeToGetRootTracks == null)
            {
                _TypeToGetRootTracks = new Editor.ConversionCache<Type, MethodInfo>((type) =>
                {
                    var method = type.GetMethod("GetRootTracks");
                    if (method != null &&
                        typeof(IEnumerable).IsAssignableFrom(method.ReturnType) &&
                        method.GetParameters().Length == 0)
                        return method;
                    else
                        return null;
                });
            }

            var getRootTracks = _TypeToGetRootTracks.Convert(asset.GetType());
            if (getRootTracks != null)
            {
                var rootTracks = getRootTracks.Invoke(asset, null);
                GatherAnimationClips(rootTracks as IEnumerable, clips);
            }
        }

        /************************************************************************************************************************/

        private static Editor.ConversionCache<Type, MethodInfo>
            _TrackAssetToGetClips,
            _TrackAssetToGetChildTracks,
            _TimelineClipToAnimationClip;

        /// <summary>Gathers all the animations in the `tracks`.</summary>
        private static void GatherAnimationClips(IEnumerable tracks, ICollection<AnimationClip> clips)
        {
            if (tracks == null)
                return;

            if (_TrackAssetToGetClips == null)
            {
                _TrackAssetToGetClips = new Editor.ConversionCache<Type, MethodInfo>((type) =>
                {
                    var method = type.GetMethod("GetClips");
                    if (method != null &&
                        typeof(IEnumerable).IsAssignableFrom(method.ReturnType) &&
                        method.GetParameters().Length == 0)
                        return method;
                    else
                        return null;
                });

                _TimelineClipToAnimationClip = new Editor.ConversionCache<Type, MethodInfo>((type) =>
                {
                    var property = type.GetProperty("animationClip");
                    if (property != null &&
                        property.PropertyType == typeof(AnimationClip))
                        return property.GetGetMethod();
                    else
                        return null;
                });

                _TrackAssetToGetChildTracks = new Editor.ConversionCache<Type, MethodInfo>((type) =>
                {
                    var method = type.GetMethod("GetChildTracks");
                    if (method != null &&
                        typeof(IEnumerable).IsAssignableFrom(method.ReturnType) &&
                        method.GetParameters().Length == 0)
                        return method;
                    else
                        return null;
                });
            }

            foreach (var track in tracks)
            {
                if (track == null)
                    continue;

                var trackType = track.GetType();

                var getClips = _TrackAssetToGetClips.Convert(trackType);
                if (getClips != null)
                {
                    var trackClips = getClips.Invoke(track, null) as IEnumerable;
                    if (trackClips != null)
                    {
                        foreach (var clip in trackClips)
                        {
                            var getClip = _TimelineClipToAnimationClip.Convert(clip.GetType());
                            if (getClip != null)
                                clips.Gather(getClip.Invoke(clip, null) as AnimationClip);
                        }
                    }
                }

                var getChildTracks = _TrackAssetToGetChildTracks.Convert(trackType);
                if (getChildTracks != null)
                {
                    var childTracks = getChildTracks.Invoke(track, null);
                    GatherAnimationClips(childTracks as IEnumerable, clips);
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Calls <see cref="Gather(ICollection{AnimationClip}, AnimationClip)"/> for each clip gathered by
        /// <see cref="IAnimationClipSource.GetAnimationClips"/>.
        /// </summary>
        public static void GatherFromSource(this ICollection<AnimationClip> clips, IAnimationClipSource source)
        {
            if (source == null)
                return;

            var list = ObjectPool.AcquireList<AnimationClip>();
            source.GetAnimationClips(list);
            clips.Gather((IEnumerable<AnimationClip>)list);
            ObjectPool.Release(list);
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Calls <see cref="Gather(ICollection{AnimationClip}, AnimationClip)"/> for each clip in the `source`,
        /// supporting both <see cref="IAnimationClipSource"/> and <see cref="IAnimationClipCollection"/>.
        /// </summary>
        public static bool GatherFromSource(this ICollection<AnimationClip> clips, object source)
        {
            if (source is AnimationClip clip)
            {
                clips.Gather(clip);
                return true;
            }

            if (source is IAnimationClipCollection collectionSource)
            {
                collectionSource.GatherAnimationClips(clips);
                return true;
            }

            if (source is IAnimationClipSource listSource)
            {
                clips.GatherFromSource(listSource);
                return true;
            }

            return false;
        }

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Calls <see cref="GatherFromSource(ICollection{AnimationClip}, object)"/> for each of the `sources`.
        /// </summary>
        public static void GatherFromSources(this ICollection<AnimationClip> clips, IList sources)
        {
            if (sources == null)
                return;

            foreach (var source in sources)
                clips.GatherFromSource(source);
        }

        /************************************************************************************************************************/
    }
}

