using System;
using System.Collections.Generic;
using PeterHan.PLib.Lighting;
using UnityEngine;

namespace ExpandedLights
{
    public static class LightDefs
    {
        public static void _LightConeHelper(GameObject source, int sourceCell, int range,
            IDictionary<int, float> brightness)
        {
            var octants = new OctantBuilder(brightness, sourceCell)
            {
                Falloff = 0.5f,
                SmoothLight = true
            };
            var rotation = source.GetComponent<Rotatable>();
            switch (rotation?.GetOrientation())
            {
                case Orientation.R90:
                    // Cone to the right
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.E_NE);
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.E_SE);
                    break;
                case Orientation.R180:
                    // Cone down
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.S_SE);
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.S_SW);
                    break;
                case Orientation.R270:
                    // Cone to the left
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.W_NW);
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.W_SW);
                    break;
                case Orientation.Neutral:
                default:
                    // Cone up
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.N_NE);
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.N_NW);
                    break;
            }
        }

        // Generate light in cone pattern according to the source's direction
        public static void LightCone(GameObject source, LightingArgs arg)
        {
            _LightConeHelper(source, arg.SourceCell, arg.Range, arg.Brightness);
        }

        // A cone that starts light one tile offset from the source.
        public static void OffsetCone(GameObject source, LightingArgs arg)
        {
            var sourceCell = arg.SourceCell;
            var rotation = source.GetComponent<Rotatable>();
            var offset_dir = new Vector2I(0, 0);
            switch (rotation?.GetOrientation())
            {
                case Orientation.R90:
                    // Cone right
                    offset_dir.X = 1;
                    break;
                case Orientation.R180:
                    // Cone down
                    offset_dir.Y = -1;
                    break;
                case Orientation.R270:
                    // Cone left
                    offset_dir.X = -1;
                    break;
                case Orientation.Neutral:
                default:
                    // Cone up
                    offset_dir.Y = 1;
                    break;
            }

            var new_sourceCell = Grid.OffsetCell(sourceCell, new CellOffset(offset_dir.X, offset_dir.Y));
            _LightConeHelper(source, new_sourceCell, arg.Range, arg.Brightness);
        }

        public static void LightCircle(GameObject source, LightingArgs arg)
        {
            var sourceCell = arg.SourceCell;
            var range = arg.Range;
            IDictionary<int, float> brightness = arg.Brightness;
            var octants = new OctantBuilder(brightness, sourceCell)
            {
                Falloff = 0.5f,
                SmoothLight = true
            };
            octants.AddOctant(range, DiscreteShadowCaster.Octant.E_NE);
            octants.AddOctant(range, DiscreteShadowCaster.Octant.E_SE);
            octants.AddOctant(range, DiscreteShadowCaster.Octant.N_NE);
            octants.AddOctant(range, DiscreteShadowCaster.Octant.N_NW);
            octants.AddOctant(range, DiscreteShadowCaster.Octant.S_SE);
            octants.AddOctant(range, DiscreteShadowCaster.Octant.S_SW);
            octants.AddOctant(range, DiscreteShadowCaster.Octant.W_NW);
            octants.AddOctant(range, DiscreteShadowCaster.Octant.W_SW);
        }

        public static void OffsetSemicircle(GameObject source, LightingArgs arg)
        {
            var rotatable = source.GetComponent<Rotatable>();
            var orient = rotatable == null ? Orientation.Neutral : rotatable.GetOrientation();
            var offset_dir = new Vector2I(0, 0);
            switch (orient)
            {
                case Orientation.R90:
                    // Cone right
                    offset_dir.X = 1;
                    break;
                case Orientation.R180:
                    // Cone down
                    offset_dir.Y = -1;
                    break;
                case Orientation.R270:
                    // Cone left
                    offset_dir.X = -1;
                    break;
                case Orientation.Neutral:
                default:
                    // Cone up
                    offset_dir.Y = 1;
                    break;
            }

            var new_sourceCell = Grid.OffsetCell(arg.SourceCell, new CellOffset(offset_dir.X, offset_dir.Y));

            _LightSemicircleHelper(source, new_sourceCell, arg.Range, orient, arg.Brightness);
        }

        // a LightSemicircle that always points down
        public static void FixedLightSemicircle(GameObject source, LightingArgs arg)
        {
            _LightSemicircleHelper(
                source,
                arg.SourceCell,
                arg.Range,
                Orientation.R180,
                arg.Brightness
            );
        }

        public static void LightSemicircle(GameObject source, LightingArgs arg)
        {
            var thing = source.GetComponent<Rotatable>();
            _LightSemicircleHelper(
                source,
                arg.SourceCell,
                arg.Range,
                thing != null ? thing.GetOrientation() : Orientation.Neutral,
                arg.Brightness);
        }

        public static void _LightSemicircleHelper(GameObject source, int sourceCell, int range, Orientation rotation,
            IDictionary<int, float> brightness)
        {
            var octants = new OctantBuilder(brightness, sourceCell)
            {
                Falloff = 0.5f,
                SmoothLight = true
            };

            switch (rotation)
            {
                case Orientation.R90:
                    // Semicircle to the right
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.N_NE);
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.E_NE);
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.E_SE);
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.S_SE);
                    break;
                case Orientation.R180:
                    // Semicircle down
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.E_SE);
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.S_SE);
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.S_SW);
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.W_SW);
                    break;
                case Orientation.R270:
                    // Semicircle to the left
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.N_NW);
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.W_NW);
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.W_SW);
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.S_SW);
                    break;
                case Orientation.Neutral:
                default:
                    // Semicircle up
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.E_NE);
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.N_NE);
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.N_NW);
                    octants.AddOctant(range, DiscreteShadowCaster.Octant.W_NW);
                    break;
            }
        }

        public static void LinearLight5(GameObject source, LightingArgs arg)
        {
            _LinearLightHelper(source, arg.SourceCell, arg.Range, arg.Brightness, 0f, 5);
        }

        public static float AdjustLightByTile(int cell, float brightness)
        {
            // If it's a Pneumatic Door, do not dim
            var obj = Grid.Objects[cell, (int) ObjectLayer.Building];
            if (obj != null)
            {
                var building = obj.GetComponent<Building>();
                if (building != null && building.Def.PrefabID == "Door") return brightness * .7f;
            }

            // Occlude slightly if mesh or airflow tile
            obj = Grid.Objects[cell, (int) ObjectLayer.FoundationTile];
            if (obj != null)
            {
                var name = obj.GetComponent<Building>().Def.PrefabID;
                if (name == "MeshTile")
                    return brightness * .9f;
                if (name == "GasPermeableMembrane") return brightness * .5f;
            }

            // Totally occlude if tile is solid.
            if (Grid.IsValidCell(cell) && !Grid.Transparent[cell] && Grid.Solid[cell]) return 0f;

            return brightness;
        }

        // Make an axis-aligned linear beam of light.
        public static void _LinearLightHelper(GameObject source, int sourceCell, int range,
            IDictionary<int, float> brightness_map, float falloff, int width)
        {
            // If width is not odd, throw exception.
            if (width < 0 || width % 2 != 1) throw new Exception("Width of LinearLight must be odd number.");

            // First determine which way we're illuminating
            var rotation = source.GetComponent<Rotatable>();
            var parallel_dir = new Vector2I(0, 0);
            var perpendicular_dir = new Vector2I(0, 0);
            switch (rotation?.GetOrientation())
            {
                case Orientation.R90:
                    // Beam right
                    parallel_dir.X = 1;
                    perpendicular_dir.Y = -1;
                    break;
                case Orientation.R180:
                    // Beam down
                    parallel_dir.Y = -1;
                    perpendicular_dir.X = -1;
                    break;
                case Orientation.R270:
                    // Beam left
                    parallel_dir.X = -1;
                    perpendicular_dir.Y = 1;
                    break;
                case Orientation.Neutral:
                default:
                    // Beam up
                    parallel_dir.Y = 1;
                    perpendicular_dir.X = 1;
                    break;
            }

            var start_offset = -width / 2;
            var end_offset = width / 2;
            // Shoot parallel rays, terminating the loop whenever light is completely occluded.
            for (var j = start_offset; j <= end_offset; j++)
            {
                var brightness = 1f;
                for (var i = 0; i < range; i++)
                {
                    var total_offset = parallel_dir * i + perpendicular_dir * j;
                    var cell = Grid.OffsetCell(sourceCell, new CellOffset(total_offset.X, total_offset.Y));
                    if (Grid.IsValidCell(cell) && brightness != 0f)
                    {
                        brightness_map[cell] = brightness;
                        //brightness = brightness * (1f - falloff);
                        brightness = AdjustLightByTile(cell, brightness) * (1f - falloff);
                    }
                    else
                    {
                        break; // go to the next ray
                    }
                }
            }
        }
    }
}