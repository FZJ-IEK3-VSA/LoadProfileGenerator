//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
// Written by Noah Pflugradt.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the distribution.
//  All advertising materials mentioning features or use of this software must display the following acknowledgement:
//  “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
//  Neither the name of the University nor the names of its contributors may be used to endorse or promote products
//  derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE UNIVERSITY 'AS IS' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, S
// PECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

#region

using JetBrains.Annotations;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

#endregion

namespace CalcPostProcessor.Helper
{
#pragma warning disable S3881 // "IDisposable" should be implemented correctly
    internal class BitmapPreparer : IDisposable
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
    {
        private readonly int _height;
        private readonly PixelFormat _pf = PixelFormats.Rgb24;
        private readonly int _rawStride;
        private readonly int _width;
        [NotNull]
        private byte[] _pixelData;

        public BitmapPreparer(int width, int height)
        {
            _width = width;
            _height = height;
#pragma warning disable VSD0045 // The operands of a divisive expression are both integers and result in an implicit rounding.
            _rawStride = (width*_pf.BitsPerPixel + 7)/8;
#pragma warning restore VSD0045 // The operands of a divisive expression are both integers and result in an implicit rounding.
            _pixelData = new byte[_rawStride*height];
        }

        public int Width => _width;

        public int Height => _height;

#pragma warning disable CC0029 // Disposables Should Call Suppress Finalize
        public void Dispose() => _pixelData = Array.Empty<byte>();
#pragma warning restore CC0029 // Disposables Should Call Suppress Finalize

        [NotNull]
        public BitmapSource GetBitmap()
            => BitmapSource.Create(_width, _height, 96, 96, _pf, null, _pixelData, _rawStride);

        public void SetPixel(int x, int y, Color c)
        {
            int xIndex = x*3;
            int yIndex = y*_rawStride;
            _pixelData[xIndex + yIndex] = c.R;
            _pixelData[xIndex + yIndex + 1] = c.G;
            _pixelData[xIndex + yIndex + 2] = c.B;
        }
    }
}