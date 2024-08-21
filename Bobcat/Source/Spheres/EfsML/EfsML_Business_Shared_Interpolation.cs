#region Using Directives
using System;
using System.Collections;
using EfsML.Enum;
using EfsML.NumericalRecipes;
#endregion Using Directives

namespace EfsML.Business
{
    #region EFS_Interp_Base
    public abstract class EFS_Interp_Base
    {
        #region Members
        protected InterpolationMethodEnum m_InterpolationMethod;
        protected InterpolationTypeEnum m_InterpolationType;
        protected double m_ResultValue;
        protected double m_ErrorResultValue;
        #endregion Members
        #region Accessors
        #region ErrorResultValue
        public double ErrorResultValue
        {
            get { return m_ErrorResultValue; }
        }
        #endregion ErrorResultValue
        #region ResultValue
        public double ResultValue
        {
            get { return m_ResultValue; }
        }
        #endregion ResultValue
        #endregion Accessors
        #region Constructors
        public EFS_Interp_Base() { }
        public EFS_Interp_Base(InterpolationMethodEnum pInterpolationMethod) : this(pInterpolationMethod, InterpolationTypeEnum.InterpolatedExtrapolated) { }
        public EFS_Interp_Base(InterpolationMethodEnum pInterpolationMethod, InterpolationTypeEnum pInterpolationType)
        {
            m_InterpolationMethod = pInterpolationMethod;
            m_InterpolationType = pInterpolationType;
        }
        #endregion Constructors
    }
    #endregion EFS_Interp_Base
    #region EFS_Interp_CubicSpline
    public class EFS_Interp_CubicSpline : EFS_Interp_Base
    {
        #region Constructors
        public EFS_Interp_CubicSpline() : base(InterpolationMethodEnum.CubicSpline, InterpolationTypeEnum.Interpolated) { }
        public EFS_Interp_CubicSpline(double[] pXa, double[] pYa, double pX) : this(pXa, pYa, pX, 1.0e30, 1.0e30) { }
        public EFS_Interp_CubicSpline(double[] pXa, double[] pYa, double pX, double pYp1, double pYpn)
            : base(InterpolationMethodEnum.CubicSpline, InterpolationTypeEnum.Interpolated)
        {
            Formula(pXa, pYa, pX, pYp1, pYpn);
        }

        #endregion Constructors
        #region Methods
        #region Formula
        public void Formula(double[] pXa, double[] pYa, double pX, double pYp1, double pYpn)
        {

            Spline spline = new Spline(pXa, pYa, pYp1, pYpn);
            Splint splint = new Splint(pXa, pYa, spline.Y2a, pX);
            m_ResultValue = splint.Y;

        }
        #endregion Formula
        #endregion Methods
    }
    #endregion EFS_Interp_CubicSpline
    #region EFS_Interp_Linear
    public class EFS_Interp_Linear : EFS_Interp_Base
    {
        #region Constructors
        public EFS_Interp_Linear(InterpolationMethodEnum pInterpolationMethod, InterpolationTypeEnum pInterpolationType, double[] pXa, double[] pYa, double pX)
            : base(pInterpolationMethod, pInterpolationType)
        {
            Locate locate = new Locate(pXa, pX);
            int j = locate.J - 1;
            if (0 < j)
            {
                //FI 20101126 
                //if (j < pXa.Length) => pXa[j + 1] plante si j est le dernier item de l'array 
                if (j < (pXa.Length - 1))
                    Formula(pX, pYa[j], pXa[j], pYa[j + 1], pXa[j + 1]);
                else
                    m_ResultValue = pYa[j];
            }
        }
        public EFS_Interp_Linear(InterpolationMethodEnum pInterpolationMethod, InterpolationTypeEnum pInterpolationType,
            double pTargetTime, double pSourceValue1, double pSourceTime1, double pSourceValue2, double pSourceTime2)
            : base(pInterpolationMethod, pInterpolationType)
        {
            Formula(pTargetTime, pSourceValue1, pSourceTime1, pSourceValue2, pSourceTime2);
        }
        #endregion Constructors
        #region Methods
        #region Formula
        public void Formula(double pTargetTime, Nullable<double> pSourceValue1, double pSourceTime1, Nullable<double> pSourceValue2, double pSourceTime2)
        {

            switch (m_InterpolationMethod)
            {
                case InterpolationMethodEnum.Linear:
                case InterpolationMethodEnum.LinearCubic:
                case InterpolationMethodEnum.LinearQuadratic:
                    LinearFormula(pTargetTime, pSourceValue1, pSourceTime1, pSourceValue2, pSourceTime2);
                    break;
                case InterpolationMethodEnum.LinearExponential:
                    LinearExponentialFormula(pTargetTime, pSourceValue1, pSourceTime1, pSourceValue2, pSourceTime2);
                    break;
                case InterpolationMethodEnum.LinearLogarithmic:
                    LnLinearFormula(pTargetTime, pSourceValue1, pSourceTime1, pSourceValue2, pSourceTime2);
                    break;
            }

        }
        #endregion Formula
        #region LinearFormula
        private void LinearFormula(double pTargetTime, Nullable<double> pSourceValue1, double pSourceTime1, Nullable<double> pSourceValue2, double pSourceTime2)
        {

            double exposant = 1;
            int sign = 1;
            if (InterpolationTypeEnum.Extrapolated == m_InterpolationType)
                sign = -1;

            switch (m_InterpolationMethod)
            {
                case InterpolationMethodEnum.Linear:
                    exposant = 1;
                    break;
                case InterpolationMethodEnum.LinearCubic:
                    exposant = 3;
                    break;
                case InterpolationMethodEnum.LinearQuadratic:
                    exposant = 2;
                    break;
            }
            double t0 = System.Math.Pow(pTargetTime, exposant);
            double t1 = System.Math.Pow(pSourceTime1, exposant);
            double t2 = System.Math.Pow(pSourceTime2, exposant);
            if (pSourceValue1.HasValue && pSourceValue2.HasValue)
                m_ResultValue = pSourceValue1.Value + (sign * (pSourceValue2.Value - pSourceValue1.Value) * (t0 - t1) / (t2 - t1));
        }
        #endregion LinearFormula
        #region LinearExponentialFormula
        private void LinearExponentialFormula(double pTargetTime, Nullable<double> pSourceValue1, double pSourceTime1, Nullable<double> pSourceValue2, double pSourceTime2)
        {

            double tba = pSourceTime2 - pSourceTime1;
            double tca = pTargetTime - pSourceTime1;
            double tbc = pSourceTime2 - pTargetTime;
            double tPrevious = (pTargetTime / pSourceTime1) * tbc / tba;
            double tNext = (pTargetTime / pSourceTime2) * tca / tba;
            if (pSourceValue1.HasValue && pSourceValue2.HasValue)
                m_ResultValue = System.Math.Pow(pSourceValue1.Value, tPrevious) * System.Math.Pow(pSourceValue2.Value, tNext);

        }
        #endregion LinearExponentialFormula
        #region LnLinearFormula
        private void LnLinearFormula(double pTargetTime, Nullable<double> pSourceValue1, double pSourceTime1, Nullable<double> pSourceValue2, double pSourceTime2)
        {

            int sign = 1;
            if (InterpolationTypeEnum.Extrapolated == m_InterpolationType)
                sign = -1;
            double x = pTargetTime / pSourceTime1;
            double y = pSourceTime2 / pSourceTime1;
            double z = System.Math.Log(x) / System.Math.Log(y);
            if (pSourceValue1.HasValue && pSourceValue2.HasValue)
                m_ResultValue = pSourceValue1.Value + (sign * (pSourceValue2.Value - pSourceValue1.Value) * z);

        }
        #endregion LnLinearFormula
        #endregion Methods
    }
    #endregion EFS_Interp_Linear
    #region EFS_Interp_Polynomial
    public class EFS_Interp_Polynomial : EFS_Interp_Base
    {
        #region Constructors
        public EFS_Interp_Polynomial() : base(InterpolationMethodEnum.Polynomial, InterpolationTypeEnum.InterpolatedExtrapolated) { }
        public EFS_Interp_Polynomial(double[] pXa, double[] pYa, double pX, int pDegree)
            : base(InterpolationMethodEnum.Polynomial, InterpolationTypeEnum.InterpolatedExtrapolated)
        {
            Formula(pXa, pYa, pX, pDegree);
        }
        #endregion Constructors
        #region Methods
        #region Formula
        public void Formula(double[] pXa, double[] pYa, double pX, int pDegree)
        {

            double[] xa = new double[pDegree + 1];
            double[] ya = new double[pDegree + 1];
            Locate locate = new Locate(pXa, pX);
            int j = locate.J - 1;
            if (0 < j)
            {
                j = System.Math.Min(System.Math.Max(j - ((pDegree) / 2), 0), pXa.Length - 1 - pDegree);
                for (int k = 0; k <= pDegree; k++)
                {
                    xa[k] = pXa[j];
                    ya[k] = pYa[j];
                    j++;
                }
                Polint polint = new Polint(xa, ya, pX);
                m_ResultValue = polint.Y;
            }

        }
        #endregion Formula
        #endregion Methods
    }
    #endregion EFS_Interp_Polynomial

    #region EFS_MxInterp_Base
    public abstract class EFS_MxInterp_Base
    {
        #region Members
        protected MatrixInterpolationMethodEnum m_InterpolationMethod;
        protected double m_ResultValue;
        protected double m_ErrorResultValue;
        #endregion Members
        #region Accessors
        #region ResultValue
        public double ResultValue
        {
            get { return m_ResultValue; }
        }
        #endregion ResultValue
        #region ErrorResultValue
        public double ErrorResultValue
        {
            get { return m_ErrorResultValue; }
        }
        #endregion ErrorResultValue
        #endregion Accessors
        #region Constructors
        public EFS_MxInterp_Base(MatrixInterpolationMethodEnum pInterpolationMethod)
        {
            m_InterpolationMethod = pInterpolationMethod;
        }
        #endregion Constructors
        #region Methods
        #region Y
        protected static double Y(double[,] pXa, double[] pYa, double[] pX)
        {
            double y = 0;
            int dim = pXa.GetLength(1);
            for (int i = 0; i < pXa.GetLength(0); i++)
            {
                bool isFound = true;
                for (int j = 0; j < dim; j++)
                {
                    isFound = isFound && ((pXa[i, j] == pX[j]));
                }
                if (isFound)
                {
                    y = pYa[i];
                    break;
                }
            }

            return y;
        }
        #endregion Y
        #endregion Methods
    }
    #endregion EFS_MxInterp_Base
    #region EFS_MxInterp_BiCubicPolynomial
    public class EFS_MxInterp_BiCubicPolynomial : EFS_MxInterp_Base
    {
        #region Constructors
        public EFS_MxInterp_BiCubicPolynomial(double[,] pXa, double[] pYa, double[] pX)
            : base(MatrixInterpolationMethodEnum.BiCubicPolynomial)
        {
            if (2 == pXa.GetLength(1))
                Formula(pXa, pYa, pX);
        }
        #endregion Constructors
        #region Methods
        #region Formula
        public void Formula(double[,] pXa, double[] pYa, double[] pX)
        {

            NearestPoints np = new NearestPoints(pXa, pX);
            double[] x1a = new double[2] { np.NearestXa[0, 0], np.NearestXa[np.DimNearestXa - 1, 0] };
            double[] x2a = new double[2] { np.NearestXa[0, 1], np.NearestXa[np.DimNearestXa - 1, 1] };

            double[,] ya = new double[2, 2];
            ya[0, 0] = Y(pXa, pYa, np.NearestX(0)); //y(x1,x2)
            ya[0, 1] = Y(pXa, pYa, np.NearestX(1)); //y(x1,x2+1)
            ya[1, 0] = Y(pXa, pYa, np.NearestX(2)); //y(x1+1,x2)
            ya[1, 1] = Y(pXa, pYa, np.NearestX(3)); //y(x1+1,x2+1)

            Polin2 polin2 = new Polin2(x1a, x2a, ya, pX[0], pX[1]);
            m_ResultValue = polin2.Y;
            m_ErrorResultValue = polin2.Error;

        }
        #endregion Formula
        #endregion Methods
    }
    #endregion EFS_MxInterp_BiCubicPolynomial
    #region EFS_MxInterp_BiCubicSpline
    public class EFS_MxInterp_BiCubicSpline : EFS_MxInterp_Base
    {
        #region Constructors
        public EFS_MxInterp_BiCubicSpline(double[,] pXa, double[] pYa, double[] pX)
            : base(MatrixInterpolationMethodEnum.BiCubicSpline)
        {
            if (2 == pXa.GetLength(1))
                Formula(pXa, pYa, pX);
        }
        #endregion Constructors
        #region Methods
        #region Formula
        public void Formula(double[,] pXa, double[] pYa, double[] pX)
        {

            #region Parameters settings
            ArrayList aX1 = new ArrayList();
            ArrayList aX2 = new ArrayList();
            for (int i = 0; i < pXa.GetLength(0); i++)
            {
                if (false == aX1.Contains(pXa[i, 0]))
                    aX1.Add(pXa[i, 0]);
                if (false == aX2.Contains(pXa[i, 1]))
                    aX2.Add(pXa[i, 1]);
            }
            double[] x1a = (double[])aX1.ToArray(typeof(Double));
            double[] x2a = (double[])aX2.ToArray(typeof(Double));
            int m = x1a.Length;
            int n = x2a.Length;
            double[,] ya = new double[m, n];
            int l = 0;
            for (int j = 0; j < m; j++)
            {
                for (int k = 0; k < n; k++)
                {
                    ya[k, j] = pYa[l];
                    l++;
                }
            }
            #endregion Parameters settings
            Formula(x1a, x2a, ya, pX[0], pX[1]);

        }
        public void Formula(double[] pX1a, double[] pX2a, double[,] pYa, double pX1, double pX2)
        {

            Splie2 splie2 = new Splie2(pX1a, pX2a, pYa);
            Splin2 splin2 = new Splin2(pX1a, pX2a, pYa, splie2.Y2a, pX1, pX2);
            m_ResultValue = splin2.Y;

        }
        #endregion Formula
        #endregion Methods
    }
    #endregion EFS_MxInterp_BiCubicSpline
    #region EFS_MxInterp_Linear
    public class EFS_MxInterp_Linear : EFS_MxInterp_Base
    {
        #region Constructors
        public EFS_MxInterp_Linear(MatrixInterpolationMethodEnum pInterpolationMethod, double[,] pXa, double[] pYa, double[] pX)
            : base(pInterpolationMethod)
        {

            switch (m_InterpolationMethod)
            {
                case MatrixInterpolationMethodEnum.Linear:
                    LinearFormula(pXa, pYa, pX);
                    break;
                case MatrixInterpolationMethodEnum.BiLinear:
                    BiLinearFormula(pXa, pYa, pX);
                    break;
                case MatrixInterpolationMethodEnum.TriLinear:
                    TriLinearFormula(pXa, pYa, pX);
                    break;
            }

        }
        #endregion Constructors
        #region Methods
        #region LinearFormula
        public void LinearFormula(double[,] pXa, double[] pYa, double[] pX)
        {

            if (1 == pXa.GetLength(1))
            {
                double[] xa = new double[pXa.GetLength(0)];
                double x = pX[0];
                for (int i = 0; i < xa.Length; i++)
                    xa[i] = pXa[i, 0];
                EFS_Interp_Linear linear = new EFS_Interp_Linear(InterpolationMethodEnum.Linear, InterpolationTypeEnum.Interpolated, xa, pYa, x);
                m_ResultValue = linear.ResultValue;
            }

        }
        #endregion LinearFormula
        #region BiLinearFormula
        public void BiLinearFormula(double[,] pXa, double[] pYa, double[] pX)
        {

            if (2 == pXa.GetLength(1))
            {
                NearestPoints np = new NearestPoints(pXa, pX);
                double[] ya = new double[4];
                ya[0] = Y(pXa, pYa, np.NearestX(0));//(x1,x2)
                ya[1] = Y(pXa, pYa, np.NearestX(2));//(x1+1,x2)
                ya[2] = Y(pXa, pYa, np.NearestX(3));//(x1+1,x2+1)
                ya[3] = Y(pXa, pYa, np.NearestX(1));//(x1,x2+1)
                double t = (pX[0] - np.NearestXa[0, 0]) / (np.NearestXa[np.DimNearestXa - 1, 0] - np.NearestXa[0, 0]);
                double u = (pX[1] - np.NearestXa[0, 1]) / (np.NearestXa[np.DimNearestXa - 1, 1] - np.NearestXa[0, 1]);
                if (Double.IsNaN(t))
                    t = 0;
                if (Double.IsNaN(u))
                    u = 0;
                double td = 1 - t;
                double ud = 1 - u;
                m_ResultValue = (td * ud * ya[0]) + (t * ud * ya[1]) + (t * u * ya[2]) + (td * u * ya[3]);
            }

        }
        #endregion BiLinearFormula
        #region TriLinearFormula
        public void TriLinearFormula(double[,] pXa, double[] pYa, double[] pX)
        {

            if (3 == pXa.GetLength(1))
            {
                NearestPoints np = new NearestPoints(pXa, pX);
                double[] ya = new double[8];
                ya[0] = Y(pXa, pYa, np.NearestX(0)); //(x1,x2,x3)
                ya[1] = Y(pXa, pYa, np.NearestX(1)); //(x1,x2,x3+1)
                ya[2] = Y(pXa, pYa, np.NearestX(5)); //(x1+1,x2,x3+1)
                ya[3] = Y(pXa, pYa, np.NearestX(4)); //(x1+1,x2,x3)
                ya[4] = Y(pXa, pYa, np.NearestX(2)); //(x1,x2+1,x3)
                ya[5] = Y(pXa, pYa, np.NearestX(3)); //(x1,x2+1,x3+1)
                ya[6] = Y(pXa, pYa, np.NearestX(7)); //(x1+1,x2+1,x3+1)
                ya[7] = Y(pXa, pYa, np.NearestX(6)); //(x1+1,x2+1,x3)

                double t = (pX[0] - np.NearestXa[0, 0]) / (np.NearestXa[np.DimNearestXa - 1, 0] - np.NearestXa[0, 0]);
                double u = (pX[1] - np.NearestXa[0, 1]) / (np.NearestXa[np.DimNearestXa - 1, 1] - np.NearestXa[0, 1]);
                double v = (pX[2] - np.NearestXa[0, 2]) / (np.NearestXa[np.DimNearestXa - 1, 2] - np.NearestXa[0, 2]);
                if (Double.IsNaN(t))
                    t = 0;
                if (Double.IsNaN(u))
                    u = 0;
                if (Double.IsNaN(v))
                    v = 0;
                double td = 1 - t;
                double ud = 1 - u;
                double vd = 1 - v;

                m_ResultValue = (td * ud * vd * ya[0]) + (t * ud * vd * ya[1]) + (t * u * vd * ya[2]) + (td * u * vd * ya[3]);
                m_ResultValue += (td * ud * v * ya[4]) + (t * ud * v * ya[5]) + (t * u * v * ya[6]) + (td * u * v * ya[7]);
            }

        }
        #endregion TriLinearFormula
        #endregion Methods
    }
    #endregion EFS_MxInterp_Linear
    #region EFS_MxInterp_NearestNeighbor
    public class EFS_MxInterp_NearestNeighbor : EFS_MxInterp_Base
    {
        #region Constructors
        public EFS_MxInterp_NearestNeighbor(double[,] pXa, double[] pYa, double[] pX)
            : base(MatrixInterpolationMethodEnum.NearestNeighbor)
        {
            Formula(pXa, pYa, pX);
        }
        #endregion Constructors
        #region Methods
        #region NearestNeighborFormula
        private void Formula(double[,] pXa, double[] pYa, double[] pX)
        {
            bool isPointsInMatrix = false;

            int nbXa = pXa.GetLength(0);
            int dim = pXa.GetLength(1);
            double[] d = new double[nbXa];
            #region Distance calcul between each point of matrix and searched point
            for (int i = 0; i < nbXa; i++)
            {
                double di = 0;
                for (int j = 0; j < dim; j++)
                {
                    di += System.Math.Pow(pX[j] - pXa[i, j], 2.0);
                }
                d[i] = System.Math.Pow(di, pYa.Length / dim);
                if (0 == d[i])
                {
                    isPointsInMatrix = true;
                    m_ResultValue = pYa[i];
                }
            }
            #endregion Distance calcul between each point of matrix and searched point
            if (false == isPointsInMatrix)
            {
                double t = 0;
                double u = 0;
                for (int i = 0; i < nbXa; i++)
                {
                    t += (pYa[i] / d[i]);
                    u += (1 / d[i]);
                }
                m_ResultValue = t / u;
            }

        }
        #endregion NearestNeighborFormula
        #endregion Methods
    }
    #endregion EFS_MxInterp_NearestNeighbor
}
