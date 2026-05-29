using System.Text;

namespace Haruka.Arcade.SEGA835Lib.Debugging {
    class Hex {
        private readonly byte[] bytes;
        private readonly int bytesPerLine;
        private readonly bool showHeader;
        private readonly bool showOffset;
        private readonly bool showAscii;

        private readonly int length;

        private int index;
        private readonly StringBuilder sb = new StringBuilder();

        private Hex(byte[] bytes, int bytesPerLine, bool showHeader, bool showOffset, bool showAscii) {
            this.bytes = bytes;
            this.bytesPerLine = bytesPerLine;
            this.showHeader = showHeader;
            this.showOffset = showOffset;
            this.showAscii = showAscii;
            length = bytes.Length;
        }

        public static string Dump(byte[] bytes, int bytesPerLine = 16, bool showHeader = true, bool showOffset = true, bool showAscii = true) {
            if (bytes == null) {
                return "<null>";
            }

            return (new Hex(bytes, bytesPerLine, showHeader, showOffset, showAscii)).Dump();
        }

        private string Dump() {
            if (showHeader) {
                WriteHeader();
            }

            WriteBody();
            return sb.ToString();
        }

        private void WriteHeader() {
            if (showOffset) {
                sb.Append("Offset(h)  ");
            }

            for (int i = 0; i < bytesPerLine; i++) {
                sb.Append($"{i & 0xFF:X2}");
                if (i + 1 < bytesPerLine) {
                    sb.Append(" ");
                }
            }

            sb.AppendLine();
        }

        private void WriteBody() {
            while (index < length) {
                if (index % bytesPerLine == 0) {
                    if (index > 0) {
                        if (showAscii) {
                            WriteAscii();
                        }

                        sb.AppendLine();
                    }

                    if (showOffset) {
                        WriteOffset();
                    }
                }

                WriteByte();
                if (index % bytesPerLine != 0 && index < length) {
                    sb.Append(" ");
                }
            }

            if (showAscii) {
                WriteAscii();
            }
        }

        private void WriteOffset() {
            sb.Append($"{index:X8}   ");
        }

        private void WriteByte() {
            sb.Append($"{bytes[index]:X2}");
            index++;
        }

        private void WriteAscii() {
            int backtrack = ((index - 1) / bytesPerLine) * bytesPerLine;
            int currentLength = index - backtrack;

            // This is to fill up last string of the dump if it's shorter than _bytesPerLine
            sb.Append(new string(' ', (bytesPerLine - currentLength) * 3));

            sb.Append("   ");
            for (int i = 0; i < currentLength; i++) {
                sb.Append(Translate(bytes[backtrack + i]));
            }
        }

        private string Translate(byte b) {
            return b < 32 ? "." : Encoding.ASCII.GetString(new[] { b });
        }
    }
}