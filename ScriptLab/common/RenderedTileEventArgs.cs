/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using System;

namespace pyrochild.effects.common
{
    public sealed class RenderedTileEventArgs : EventArgs
    {
        private PdnRegion renderedRegion;
        private int tileCount;
        private int tileNumber;
        
        public RenderedTileEventArgs(PdnRegion renderedRegion, int tileCount, int tileNumber)
        {
            this.renderedRegion = renderedRegion;
            this.tileCount = tileCount;
            this.tileNumber = tileNumber;
        }
        
        public PdnRegion RenderedRegion
        {
            get
            {
                return renderedRegion;
            }
        }

        public int TileCount
        {
            get
            {
                return tileCount;
            }
        }

        public int TileNumber
        {
            get
            {
                return tileNumber;
            }
        }
    }
}
