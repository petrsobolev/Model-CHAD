﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CHAD.Model.ClimateModule
{
    public class Climate : IEnumerable<DailyClimate>
    {
        #region Fields

        private readonly Parameters _parameters;
        private readonly IEnumerable<ClimateForecast> _climateForecasts;

        private readonly List<DailyClimate> _dailyClimates;
        private readonly List<DroughtLevel> _droughtLevels;

        private readonly Random _temperatureRandom;
        private readonly Random _precipitationRandom;


        #endregion

        #region Constructors

        public Climate(Parameters parameters, IEnumerable<ClimateForecast> climateForecasts, IEnumerable<DroughtLevel> droughtLevel)
        {
            _parameters = parameters;
            _climateForecasts = climateForecasts;
            _dailyClimates = new List<DailyClimate>();
            _droughtLevels = new List<DroughtLevel>(droughtLevel);

            _temperatureRandom = new Random();
            _precipitationRandom = new Random();
        }

        #endregion

        #region Public Interface

        public void ProcessSeason(int seasonNumber)
        {
            _dailyClimates.Clear();

            foreach (var climateForecast in _climateForecasts)
            {
                var temperature = GetRandomTemperature(seasonNumber, climateForecast);

                var precipitation = GetRandomPrecipitation(seasonNumber, climateForecast);

                _dailyClimates.Add(new DailyClimate(climateForecast.Day, temperature, precipitation));
            }
        }

        public DailyClimate GetDailyClimate(int day)
        {
            return _dailyClimates.FirstOrDefault(dc => dc.Day == day);
        }

        public IEnumerator<DailyClimate> GetEnumerator()
        {
            return _dailyClimates.GetEnumerator();
        }

        #endregion

        #region Interface Implementations

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region All other members

        private double GetDroughtLevel(int seasonNumber)
        {
            return _droughtLevels.First(dl => dl.SeasonNumber == seasonNumber).Value;
        }

        private double GetRandomTemperature(int seasonNumber, ClimateForecast climateForecast)
        {
            var u1 = 1d - _temperatureRandom.NextDouble();
            var u2 = 1d - _temperatureRandom.NextDouble();
            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

            var mean = Math.Pow(_parameters.ClimateChangeTempMean, seasonNumber) * climateForecast.TempMean;
            var deviation = Math.Pow(_parameters.ClimateChangeTempSD, seasonNumber) * climateForecast.TempSD;

            var temperature = mean + deviation * randStdNormal;

            return Math.Round(temperature, 2);
        }

        private double GetRandomPrecipitation(int seasonNumber, ClimateForecast climateForecast)
        {
            var u1 = 1d - _precipitationRandom.NextDouble();
            var u2 = 1d - _precipitationRandom.NextDouble();
            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

            var mean = Math.Pow(_parameters.ClimateChangePrecipMean, seasonNumber) * climateForecast.PrecipMean;
            var deviation = Math.Pow(_parameters.ClimateChangePrecipSD, seasonNumber) * climateForecast.PrecipSD;

            var precipitation = GetDroughtLevel(seasonNumber) * (mean + deviation * randStdNormal);

            return Math.Round(precipitation, 2);
        }

        #endregion
    }
}