﻿using System.Collections.Generic;

namespace Model
{
    public interface IStorageService
    {
        #region Public Interface

        IEnumerable<Configuration> GetConfigurations();

        void SaveLogs(string path, SimpleLogger logger);

        void SaveClimate(string path, IEnumerable<Climate> climate);

        void SaveHydrology(string path, List<Hydrology> hydrology, IEnumerable<Field> inputFieldSize);

        Configuration GetConfiguration(Configuration configuration);

        void SaveConfiguration(Configuration configuration, bool rewrite);

        void SaveSimulationResult(SimulationResult simulationResult,
            SimulationResultPart simulationResultPart = SimulationResultPart.Parameters | SimulationResultPart.Climate |
                                                        SimulationResultPart.Hydrology);

        #endregion
    }
}