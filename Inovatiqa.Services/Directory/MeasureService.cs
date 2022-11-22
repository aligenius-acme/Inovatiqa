using Inovatiqa.Core;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Directory.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Directory
{
    public partial class MeasureService : IMeasureService
    {
        #region Fields

        private readonly IRepository<MeasureDimension> _measureDimensionRepository;
        private readonly IRepository<MeasureWeight> _measureWeightRepository;

        #endregion

        #region Ctor

        public MeasureService(IRepository<MeasureDimension> measureDimensionRepository,
            IRepository<MeasureWeight> measureWeightRepository)
        {
            _measureDimensionRepository = measureDimensionRepository;
            _measureWeightRepository = measureWeightRepository;
        }

        #endregion

        #region Methods

        #region Dimensions

        public virtual void DeleteMeasureDimension(MeasureDimension measureDimension)
        {
            if (measureDimension == null)
                throw new ArgumentNullException(nameof(measureDimension));

            _measureDimensionRepository.Delete(measureDimension);

            //_eventPublisher.EntityDeleted(measureDimension);
        }

        public virtual MeasureDimension GetMeasureDimensionById(int measureDimensionId)
        {
            if (measureDimensionId == 0)
                return null;

            return _measureDimensionRepository.GetById(measureDimensionId);
        }

        public virtual MeasureDimension GetMeasureDimensionBySystemKeyword(string systemKeyword)
        {
            if (string.IsNullOrEmpty(systemKeyword))
                return null;

            var measureDimensions = GetAllMeasureDimensions();
            foreach (var measureDimension in measureDimensions)
                if (measureDimension.SystemKeyword.ToLowerInvariant() == systemKeyword.ToLowerInvariant())
                    return measureDimension;
            return null;
        }

        public virtual IList<MeasureDimension> GetAllMeasureDimensions()
        {
            var query = from md in _measureDimensionRepository.Query()
                orderby md.DisplayOrder, md.Id
                select md;
            var measureDimensions = query.ToList();

            return measureDimensions;
        }

        public virtual void InsertMeasureDimension(MeasureDimension measure)
        {
            if (measure == null)
                throw new ArgumentNullException(nameof(measure));

            _measureDimensionRepository.Insert(measure);

            //_eventPublisher.EntityInserted(measure);
        }

        public virtual void UpdateMeasureDimension(MeasureDimension measure)
        {
            if (measure == null)
                throw new ArgumentNullException(nameof(measure));

            _measureDimensionRepository.Update(measure);

            //_eventPublisher.EntityUpdated(measure);
        }

        public virtual decimal ConvertDimension(decimal value,
            MeasureDimension sourceMeasureDimension, MeasureDimension targetMeasureDimension, bool round = true)
        {
            if (sourceMeasureDimension == null)
                throw new ArgumentNullException(nameof(sourceMeasureDimension));

            if (targetMeasureDimension == null)
                throw new ArgumentNullException(nameof(targetMeasureDimension));

            var result = value;
            if (result != decimal.Zero && sourceMeasureDimension.Id != targetMeasureDimension.Id)
            {
                result = ConvertToPrimaryMeasureDimension(result, sourceMeasureDimension);
                result = ConvertFromPrimaryMeasureDimension(result, targetMeasureDimension);
            }

            if (round)
                result = Math.Round(result, 2);

            return result;
        }

        public virtual decimal ConvertToPrimaryMeasureDimension(decimal value,
            MeasureDimension sourceMeasureDimension)
        {
            if (sourceMeasureDimension == null)
                throw new ArgumentNullException(nameof(sourceMeasureDimension));

            var result = value;
            var baseDimensionIn = GetMeasureDimensionById(InovatiqaDefaults.BaseDimensionId);
            if (result == decimal.Zero || sourceMeasureDimension.Id == baseDimensionIn.Id) 
                return result;

            var exchangeRatio = sourceMeasureDimension.Ratio;
            if (exchangeRatio == decimal.Zero)
                throw new InovatiqaException($"Exchange ratio not set for dimension [{sourceMeasureDimension.Name}]");
            result = result / exchangeRatio;

            return result;
        }

        public virtual decimal ConvertFromPrimaryMeasureDimension(decimal value,
            MeasureDimension targetMeasureDimension)
        {
            if (targetMeasureDimension == null)
                throw new ArgumentNullException(nameof(targetMeasureDimension));

            var result = value;
            var baseDimensionIn = GetMeasureDimensionById(InovatiqaDefaults.BaseDimensionId);
            if (result == decimal.Zero || targetMeasureDimension.Id == baseDimensionIn.Id) 
                return result;

            var exchangeRatio = targetMeasureDimension.Ratio;
            if (exchangeRatio == decimal.Zero)
                throw new InovatiqaException($"Exchange ratio not set for dimension [{targetMeasureDimension.Name}]");
            result = result * exchangeRatio;

            return result;
        }

        #endregion

        #region Weights

        public virtual void DeleteMeasureWeight(MeasureWeight measureWeight)
        {
            if (measureWeight == null)
                throw new ArgumentNullException(nameof(measureWeight));

            _measureWeightRepository.Delete(measureWeight);

            //_eventPublisher.EntityDeleted(measureWeight);
        }

        public virtual MeasureWeight GetMeasureWeightById(int measureWeightId)
        {
            if (measureWeightId == 0)
                return null;

            return _measureWeightRepository.GetById(measureWeightId);
        }

        public virtual MeasureWeight GetMeasureWeightBySystemKeyword(string systemKeyword)
        {
            if (string.IsNullOrEmpty(systemKeyword))
                return null;

            var measureWeights = GetAllMeasureWeights();
            foreach (var measureWeight in measureWeights)
                if (measureWeight.SystemKeyword.ToLowerInvariant() == systemKeyword.ToLowerInvariant())
                    return measureWeight;
            return null;
        }

        public virtual IList<MeasureWeight> GetAllMeasureWeights()
        {
            var query = from mw in _measureWeightRepository.Query()
                orderby mw.DisplayOrder, mw.Id
                select mw;
            var measureWeights = query.ToList();

            return measureWeights;
        }

        public virtual void InsertMeasureWeight(MeasureWeight measure)
        {
            if (measure == null)
                throw new ArgumentNullException(nameof(measure));

            _measureWeightRepository.Insert(measure);

            //_eventPublisher.EntityInserted(measure);
        }

        public virtual void UpdateMeasureWeight(MeasureWeight measure)
        {
            if (measure == null)
                throw new ArgumentNullException(nameof(measure));

            _measureWeightRepository.Update(measure);

            //_eventPublisher.EntityUpdated(measure);
        }

        public virtual decimal ConvertWeight(decimal value,
            MeasureWeight sourceMeasureWeight, MeasureWeight targetMeasureWeight, bool round = true)
        {
            if (sourceMeasureWeight == null)
                throw new ArgumentNullException(nameof(sourceMeasureWeight));

            if (targetMeasureWeight == null)
                throw new ArgumentNullException(nameof(targetMeasureWeight));

            var result = value;
            if (result != decimal.Zero && sourceMeasureWeight.Id != targetMeasureWeight.Id)
            {
                result = ConvertToPrimaryMeasureWeight(result, sourceMeasureWeight);
                result = ConvertFromPrimaryMeasureWeight(result, targetMeasureWeight);
            }

            if (round)
                result = Math.Round(result, 2);

            return result;
        }

        public virtual decimal ConvertToPrimaryMeasureWeight(decimal value, MeasureWeight sourceMeasureWeight)
        {
            if (sourceMeasureWeight == null)
                throw new ArgumentNullException(nameof(sourceMeasureWeight));

            var result = value;
            var baseWeightIn = GetMeasureWeightById(InovatiqaDefaults.BaseWeightId);
            if (result == decimal.Zero || sourceMeasureWeight.Id == baseWeightIn.Id)
                return result;

            var exchangeRatio = sourceMeasureWeight.Ratio;
            if (exchangeRatio == decimal.Zero)
                throw new InovatiqaException($"Exchange ratio not set for weight [{sourceMeasureWeight.Name}]");
            result = result / exchangeRatio;

            return result;
        }

        public virtual decimal ConvertFromPrimaryMeasureWeight(decimal value,
            MeasureWeight targetMeasureWeight)
        {
            if (targetMeasureWeight == null)
                throw new ArgumentNullException(nameof(targetMeasureWeight));

            var result = value;
            var baseWeightIn = GetMeasureWeightById(InovatiqaDefaults.BaseWeightId);
            if (result == decimal.Zero || targetMeasureWeight.Id == baseWeightIn.Id) 
                return result;

            var exchangeRatio = targetMeasureWeight.Ratio;
            if (exchangeRatio == decimal.Zero)
                throw new InovatiqaException($"Exchange ratio not set for weight [{targetMeasureWeight.Name}]");
            result = result * exchangeRatio;

            return result;
        }

        #endregion

        #endregion
    }
}