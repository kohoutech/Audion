/* ----------------------------------------------------------------------------
Transonic Patch Library
Copyright (C) 1995-2019  George E Greaney

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
----------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Origami.ENAML;

//interface for patch canvas communication with it's backing model

namespace Transonic.Patch
{
    public interface IPatchModel
    {
        //allow the backing model to create a subclass of a patch box using the model data stored in palette item's tag field
        PatchBox getPatchBox(PaletteItem item);

        //allow the backing model to create a subclass of a patch wire & connect it to source & dest units in the model
        PatchWire getPatchWire(PatchPanel source, PatchPanel dest);

        //removes unit from backing model's patch graph
        void removePatchBox(PatchBox box);

        //allows the backing model to disconnect two units
        void removePatchWire(PatchWire wire);

        //- loading/saving to patch files -------------------------------------

        //load model specific data from the patch file stored in ENAML format
        void loadPatchData(EnamlData data);

        PatchBox loadPatchBox(EnamlData data, String path);

        PatchWire loadPatchWire(EnamlData data, String path, PatchPanel source, PatchPanel dest);

        //save model specific data to the patch file stored in ENAML format
        void savePatchData(EnamlData data);

        //save model specific box subclass to patch file
        void savePatchBox(EnamlData data, String path, PatchBox box);

        //save model specific wire subclass to patch file
        void savePatchWire(EnamlData data, String path, PatchWire wire);

        //let the model know the patch has changed
        void patchHasChanged();

        //let the model know the patch has been cleared
        void patchHasBeenCleared();

    }
}
