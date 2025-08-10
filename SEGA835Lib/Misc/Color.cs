namespace Haruka.Arcade.SEGA835Lib.Misc {
    /// <summary>
    /// Class holding a color. (System.Drawing replacement)
    /// </summary>
    public struct Color {
        /// <summary>
        /// The red value.
        /// </summary>
        public byte R;

        /// <summary>
        /// The green value.
        /// </summary>
        public byte G;

        /// <summary>
        /// The blue value.
        /// </summary>
        public byte B;

        /// <summary>
        /// Red.
        /// </summary>
        public static Color Red {
            get {
                return FromArgb(255, 0, 0);
            }
        }

        /// <summary>
        /// Green.
        /// </summary>
        public static Color Green {
            get {
                return FromArgb(0, 255, 0);
            }
        }

        /// <summary>
        /// Blue.
        /// </summary>
        public static Color Blue {
            get {
                return FromArgb(0, 0, 255);
            }
        }

        /// <summary>
        /// White.
        /// </summary>
        public static Color White {
            get {
                return FromArgb(255, 255, 255);
            }
        }

        /// <summary>
        /// Black.
        /// </summary>
        public static Color Black {
            get {
                return FromArgb(0, 0, 0);
            }
        }

        /// <summary>
        /// Creates a new color object.
        /// </summary>
        /// <param name="r">red</param>
        /// <param name="g">green</param>
        /// <param name="b">blue</param>
        /// <returns>a new color object</returns>
        public static Color FromArgb(byte r, byte g, byte b) {
            return new Color() {
                R = r,
                G = g,
                B = b
            };
        }
    }
}