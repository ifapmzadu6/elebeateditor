using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;


static class WindowUtil
{


    #region public struct WmRect

    /// <summary>
    /// 
    /// </summary>
    public struct WmRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public static implicit operator Rectangle(WmRect r)
        {
            return new Rectangle(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
        }

        public static implicit operator WmRect(Rectangle r)
        {
            return new WmRect
            {
                Top = r.Top,
                Bottom = r.Bottom,
                Left = r.Left,
                Right = r.Right
            };
        }

        public override string ToString()
        {
            return string.Format(
                "Left {0}, Top {1}, Right {2}, Bottom {3}",
                this.Left.ToString(),
                this.Top.ToString(),
                this.Right.ToString(),
                this.Bottom.ToString());
        }
    }

    #endregion


    #region public enum WmSz

    /// <summary>
    /// 
    /// </summary>
    public enum WmSz
    {
        Left = 1,
        Right = 2,
        Top = 3,
        Bottom = 6,

        TopLeft = Top + Left,
        TopRight = Top + Right,
        BottomLeft = Bottom + Left,
        BottomRight = Bottom + Right,
    }

    #endregion


    #region public static void AspectRatioSizeWinProc( this Form form, ... )

    /// <summary>
    /// ウィンドウ・プロシージャ内で呼び出すことで、
    /// マウスでウィンドウの縁をドラッグしてフォームのサイズを変更したときに、
    /// ウィンドウサイズが指定したアスペクト比を維持するようになる。
    /// </summary>
    /// <param name="form">サイズが変更されるウィンドウ</param>
    /// <param name="m">ウィンドウ・プロシージャに渡されたWindowsメッセージ</param>
    /// <param name="aspect">ウィンドウサイズのアスペクト比（幅/高さ）</param>
    /// <param name="clientSize">クライアント領域のアスペクト比を一定に保つようにする場合はtrue。
    /// falseを指定すると、ウィンドウの境界線やタイトルバーも含めたウィンドウ全体のサイズのアスペクト比を保つようにする。</param>
    /// <remarks>
    /// <example>
    /// <code>
    /// class Form1 : Form
    /// {
    ///     protected override void WndProc( ref Message m )
    ///     {
    ///         this.AspectRatioSizeWinProc( ref m, 16f / 9f, true );
    ///         base.WndProc( ref m );
    ///     }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    public static void AspectRatioSizeWndProc(this Form form, ref Message m, float aspect, bool clientSize)
    {
        const int WM_SIZING = 0x214;

        if (m.Msg == WM_SIZING)
        {
            if (aspect > 0f)
            {
                //画面上での、ウィンドウの上下左右の座標
                WmRect rc = (WmRect)Marshal.PtrToStructure(m.LParam, typeof(WmRect));
                WmSz res = (WmSz)m.WParam.ToInt32();

                switch (res)
                {
                    case WmSz.Left:
                    case WmSz.Right:
                        {
                            int w = rc.Right - rc.Left;
                            int h;
                            if (clientSize)
                            {
                                Size borders = Size.Subtract(form.Size, form.ClientSize);

                                w -= borders.Width;
                                h = (int)(w / aspect) + borders.Height;
                            }
                            else
                            {
                                h = (int)(w / aspect);
                            }

                            rc.Bottom = rc.Top + h;
                            Marshal.StructureToPtr(rc, m.LParam, true);

                            break;
                        }
                    case WmSz.Top:
                    case WmSz.Bottom:
                        {
                            int h = rc.Bottom - rc.Top;
                            int w;

                            if (clientSize)
                            {
                                Size borders = Size.Subtract(form.Size, form.ClientSize);
                                h -= borders.Height;
                                w = (int)(h * aspect) + borders.Width;
                            }
                            else
                            {
                                w = (int)(h * aspect);
                            }

                            rc.Right = rc.Left + w;

                            break;
                        }
                    case WmSz.TopLeft:
                    case WmSz.TopRight:
                        {
                            int recW = rc.Right - rc.Left;
                            int recH = rc.Bottom - rc.Top;

                            int w, h;

                            if (clientSize)
                            {
                                Size borders = Size.Subtract(form.Size, form.ClientSize);
                                recW -= borders.Width;
                                recH -= borders.Height;

                                w = (int)(recH * aspect) + borders.Width;
                                h = (int)(recW / aspect) + borders.Height;
                            }
                            else
                            {
                                w = (int)(recH * aspect);
                                h = (int)(recW / aspect);
                            }

                            int dh = recW * recW + h * h;
                            int dw = recH * recH + w * w;

                            if (dh > dw)
                            {
                                rc.Top = rc.Bottom - h;
                            }
                            else
                            {
                                if (res == WmSz.TopLeft)
                                {
                                    rc.Left = rc.Right - w;
                                }
                                else if (res == WmSz.TopRight)
                                {
                                    rc.Right = rc.Left + w;
                                }
                            }

                            break;
                        }

                    case WmSz.BottomLeft:
                    case WmSz.BottomRight:
                        {
                            int recW = rc.Right - rc.Left;
                            int recH = rc.Bottom - rc.Top;

                            int w, h;

                            if (clientSize)
                            {
                                Size borders = Size.Subtract(form.Size, form.ClientSize);
                                recW -= borders.Width;
                                recH -= borders.Height;

                                w = (int)(recH * aspect) + borders.Width;
                                h = (int)(recW / aspect) + borders.Height;
                            }
                            else
                            {
                                w = (int)(recH * aspect);
                                h = (int)(recW / aspect);
                            }

                            int dh = recW * recW + h * h;
                            int dw = recH * recH + w * w;

                            if (dh > dw)
                            {
                                rc.Bottom = rc.Top + h;
                            }
                            else
                            {
                                if (res == WmSz.BottomLeft)
                                {
                                    rc.Left = rc.Right - w;
                                }
                                else if (res == WmSz.BottomRight)
                                {
                                    rc.Right = rc.Left + w;
                                }
                            }

                            break;
                        }
                    default:
                        break;
                }

                Marshal.StructureToPtr(rc, m.LParam, true);
            }
        }
    }

    #endregion


}
