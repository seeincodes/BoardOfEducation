namespace BoardOfEducation.Validators
{
    public static class ValidatorFactory
    {
        public static IPuzzleValidator Create(LevelConfig config)
        {
            return config.conceptType switch
            {
                ConceptType.Sequence => new SequenceValidator(config),
                ConceptType.Procedure => new ProcedureValidator(config),
                ConceptType.Loop => new LoopValidator(config),
                ConceptType.Conditional => new ConditionalValidator(config),
                _ => new SequenceValidator(config)
            };
        }
    }
}
