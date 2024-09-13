using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.Constants
{
    public static class PartialPlayerConstants
    {
        public static string[] PlayerMeshSetNames = Enum.GetNames(typeof(PlayerMeshSet));
    }

    public enum PlayerComponentModel
    {
        Starter
    }
    public enum PlayerMeshSet
    {
        None,
        StarterMeshSet
    }
    public enum PlayerMesh
    {
        Armature_Mesh
    }
    public enum PlayerSubmesh
    {
        M_Armature_Arms,
        M_Armature_Body,
        M_Armature_Legs
    }
}
