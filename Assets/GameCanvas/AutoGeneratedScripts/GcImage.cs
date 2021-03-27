/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2021 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
#nullable enable

namespace GameCanvas
{
    public readonly partial struct GcImage : System.IEquatable<GcImage>
    {
        internal const int __Length__ = 6;
        public static readonly GcImage Cow = new GcImage("Cow", 256, 256);
        public static readonly GcImage Factory = new GcImage("Factory", 256, 256);
        public static readonly GcImage Planet = new GcImage("Planet", 256, 256);
        public static readonly GcImage Rocket = new GcImage("Rocket", 256, 256);
        public static readonly GcImage RocketFire = new GcImage("RocketFire", 256, 256);
        public static readonly GcImage Shit = new GcImage("Shit", 256, 256);
    }
}
