using System.Collections.Generic;
using Automation;
using Common.Enums;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcSubAffordanceDto {
        [NotNull]
        public string Name { get; }

        public int ID { get; }
        [NotNull]
        public string LocName { get; }

        [NotNull]
        public StrGuid LocGuid { get; }
        [NotNull][ItemNotNull]
        public List<CalcDesireDto> Satisfactionvalues { get; }

        public int MiniumAge { get; }
        public int MaximumAge { get; }
        public int Delaytimesteps { get; }
        public PermittedGender PermittedGender { get; }
        [NotNull]
        public string AffCategory { get; }

        public bool IsInterruptable { get; }
        public bool IsInterrupting { get; }
        [NotNull]
        [ItemNotNull]
        public List<CalcAffordanceVariableOpDto> VariableOps { get; }

        public int Weight { get; }
        [NotNull]
        public string SourceTrait { get; }
        [NotNull]
        public StrGuid Guid { get; }

        public CalcSubAffordanceDto([NotNull] string name, int id, [NotNull] string locName, [NotNull] StrGuid locGuid,
                                    [NotNull][ItemNotNull] List<CalcDesireDto> satisfactionvalues,
                                    int miniumAge, int maximumAge, int delaytimesteps, PermittedGender permittedGender,
                                    [NotNull] string affCategory,
                                    bool isInterruptable, bool isInterrupting,
                                    [ItemNotNull][NotNull] List<CalcAffordanceVariableOpDto> variableOps, int weight, [NotNull] string sourceTrait,
                                    [NotNull] StrGuid guid)
        {
            Name = name;
            ID = id;
            LocName = locName;
            LocGuid = locGuid;
            Satisfactionvalues = satisfactionvalues;
            MiniumAge = miniumAge;
            MaximumAge = maximumAge;
            Delaytimesteps = delaytimesteps;
            PermittedGender = permittedGender;
            AffCategory = affCategory;
            IsInterruptable = isInterruptable;
            IsInterrupting = isInterrupting;
            VariableOps = variableOps;
            Weight = weight;
            SourceTrait = sourceTrait;
            Guid = guid;
        }
    }
}