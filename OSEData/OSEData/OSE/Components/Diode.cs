﻿namespace OSEData.OSE.Components
{
    public class Diode : ElectricalComponent, IElectronicComponent
    {
        public IComponentParameters ParametersModel => new DiodeParameters();
        public IComponentRuntimeParameters ParametersRuntime => new DiodeRuntimeParameters();
    }

    public class DiodeParameters : IComponentParameters
    {
        public float ActivationEnergy { get; set; }
        public float BreakdownCurrent { get; set; }
        public float BreakdownVoltage { get; set; }
        public float DepletionCapCoefficient { get; set; }
        public float EmissionCoefficient { get; set; }
        public float FlickerNoiseCoefficient { get; set; }
        public float FlickerNoiseExponent { get; set; }
        public float GradingCoefficient { get; set; }
        public float JunctionCap { get; set; }
        public float JunctionPotential { get; set; }
        public float NominalTemperature { get; set; }
        public float NominalTemperatureCelsius { get; set; }
        public float Resistance { get; set; }
        public float SaturationCurrent { get; set; }
        public float SaturationCurrentExp { get; set; }
        public float TransitTime { get; set; }
    }

    public class DiodeRuntimeParameters : IComponentRuntimeParameters
    {
        public float Area { get; set; }
        public float InitCond { get; set; }
        public bool Off { get; set; }
        public float ParallelMultiplier { get; set; }
        public float SeriesMultiplier { get; set; }
        public float Temperature { get; set; }
        public float TemperatureCelsius { get; set; }
    }
}