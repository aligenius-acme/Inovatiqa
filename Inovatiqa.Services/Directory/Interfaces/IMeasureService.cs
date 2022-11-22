using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Directory.Interfaces
{
    public partial interface IMeasureService
    {
        void DeleteMeasureDimension(MeasureDimension measureDimension);

        MeasureDimension GetMeasureDimensionById(int measureDimensionId);

        MeasureDimension GetMeasureDimensionBySystemKeyword(string systemKeyword);

        IList<MeasureDimension> GetAllMeasureDimensions();

        void InsertMeasureDimension(MeasureDimension measure);

        void UpdateMeasureDimension(MeasureDimension measure);

        decimal ConvertDimension(decimal value,
            MeasureDimension sourceMeasureDimension, MeasureDimension targetMeasureDimension, bool round = true);

        decimal ConvertToPrimaryMeasureDimension(decimal value,
            MeasureDimension sourceMeasureDimension);

        decimal ConvertFromPrimaryMeasureDimension(decimal value,
            MeasureDimension targetMeasureDimension);

        void DeleteMeasureWeight(MeasureWeight measureWeight);

        MeasureWeight GetMeasureWeightById(int measureWeightId);

        MeasureWeight GetMeasureWeightBySystemKeyword(string systemKeyword);

        IList<MeasureWeight> GetAllMeasureWeights();

        void InsertMeasureWeight(MeasureWeight measure);

        void UpdateMeasureWeight(MeasureWeight measure);

        decimal ConvertWeight(decimal value,
            MeasureWeight sourceMeasureWeight, MeasureWeight targetMeasureWeight, bool round = true);

        decimal ConvertToPrimaryMeasureWeight(decimal value, MeasureWeight sourceMeasureWeight);

        decimal ConvertFromPrimaryMeasureWeight(decimal value,
            MeasureWeight targetMeasureWeight);
    }
}