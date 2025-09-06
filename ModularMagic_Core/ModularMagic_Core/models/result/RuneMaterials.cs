using UnityEngine;

namespace ModularMagic_Core.Models
{
    internal class RuneMaterials
    {
        public Material grausten;
        public Material marble;
        public Material stone;
        public Material woodOff;
        public Material wood;

        public RuneMaterials(Material woodOff, Material wood, Material stone, Material marble, Material grausten)
        {
            this.grausten = grausten;
            this.marble = marble;
            this.stone = stone;
            this.wood = wood;
            this.woodOff = woodOff;
        }
    }
}
