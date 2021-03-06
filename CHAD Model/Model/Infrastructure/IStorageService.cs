﻿using System.Collections.Generic;
using CHAD.Model.SimulationResults;

namespace CHAD.Model.Infrastructure
{
    public interface IStorageService
    {
        #region Public Interface

        Configuration GetConfiguration(Configuration configuration);

        IEnumerable<Configuration> GetConfigurations();

        Configuration GetDefaultConfiguration();

        void SaveConfiguration(Configuration configuration, bool rewrite);

        void SaveSimulationResult(SimulationResult simulationResult);

        void SaveAgroHydrologyResults(SimulationInfo simulationInfo, IEnumerable<AgroHydrologyRecord> records);

        #endregion
    }
}