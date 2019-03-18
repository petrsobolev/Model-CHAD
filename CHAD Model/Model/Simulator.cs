﻿using System;
using System.Threading;

namespace Model
{
    public class Simulator
    {
        private readonly ILoggerFactory _loggerFactory;

        #region Fields

        private SimulatorStatus _status;

        #endregion

        #region Constructors

        public Simulator(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        #endregion

        #region Public Interface

        public event Action<SimulationResult> SimulationResultObtained;

        public AgroHydrology AgroHydrology { get; private set; }

        public event Action StatusChanged;

        public Configuration Configuration { get; private set; }

        public SimulatorStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                StatusChanged?.Invoke();
            }
        }

        public void Start()
        {
            var previousStatus = Status;

            Status = SimulatorStatus.Run;

            if (previousStatus == SimulatorStatus.Run)
                return;

            if (previousStatus == SimulatorStatus.OnPaused)
                return;

            Simulate();

            Stop();
        }

        public void Stop()
        {
            if (Status == SimulatorStatus.Stopped)
                return;

            CurrentSimulation = 0;
            CurrentSeason = 0;
            CurrentDay = 0;

            Status = SimulatorStatus.Stopped;
        }

        public void Pause()
        {
            if (Status == SimulatorStatus.Stopped || Status == SimulatorStatus.OnPaused)
                return;

            Status = SimulatorStatus.OnPaused;
        }

        public int CurrentSimulation { get; private set; }

        public int CurrentSeason { get; private set; }

        public int CurrentDay { get; private set; }

        public void SetConfiguration(Configuration configuration)
        {
            if (Status != SimulatorStatus.Stopped)
                throw new InvalidOperationException("Unable to change configuration while simulator is working");

            Configuration = configuration;
        }

        #endregion

        #region All other members

        private void Simulate2()
        {
            for (var seasonNumber = 1; seasonNumber < int.MaxValue; seasonNumber++)
            {
                CheckStatus();
                CurrentSeason = seasonNumber;

                for (var dayNumber = 1; dayNumber < int.MaxValue; dayNumber++)
                {
                    CheckStatus();
                    CurrentDay = dayNumber;
                }
            }
        }

        private void Simulate()
        {
            var simulationSession = MakeSimulationSession();

            for (var simulationNumber = 1;
                simulationNumber <= Configuration.Parameters.NumOfSimulations;
                simulationNumber++)
            {
                CheckStatus();
                CurrentSimulation = simulationNumber;

                var logger = _loggerFactory.MakeLogger(Configuration.Name, simulationSession, simulationNumber);

                for (var seasonNumber = 1; seasonNumber <= Configuration.Parameters.NumOfSeasons; seasonNumber++)
                {
                    CheckStatus();
                    CurrentSeason = seasonNumber;

                    AgroHydrology = new AgroHydrology(logger, Configuration.Parameters, Configuration.ClimateList,
                        Configuration.Fields, Configuration.CropEvapTransList);

                    for (var dayNumber = 1; dayNumber < Configuration.DaysCount; dayNumber++)
                    {
                        CheckStatus();
                        CurrentDay = dayNumber;

                        AgroHydrology.ProcessDay(dayNumber);
                    }
                }

                var simulationResults = new SimulationResult(simulationSession, Configuration, simulationNumber, AgroHydrology);
                RaiseSimulationResultObtained(simulationResults);
            }
        }

        private string MakeSimulationSession()
        {
            return DateTimeOffset.Now.ToString("yyyy.MM.dd -- HH-mm-ss");
        }

        private void CheckStatus()
        {
            while (Status == SimulatorStatus.OnPaused)
                Thread.Sleep(250);

            if (Status == SimulatorStatus.Stopped)
                Thread.CurrentThread.Abort();
        }

        private void RaiseSimulationResultObtained(SimulationResult simulationResult)
        {
            SimulationResultObtained?.Invoke(simulationResult);
        }

        #endregion
    }
}