using System.Collections.Generic;
using Klei.AI;

namespace DiseasesReimagined
{
    class SlimeLethalSickness : Sickness
    {
        public const string ID = "SlimeLethal";

        public SlimeLethalSickness()
            : base(ID, SicknessType.Ailment, Severity.Major, 0.00025f,
                new List<InfectionVector>
                {
                    InfectionVector.Inhalation
                }, 3700f)
        {
            fatalityDuration = 3590f;
            AddSicknessComponent(new ModifyParentTimeComponent(SlimeSickness.ID, .5f));
        }
    }
} 
