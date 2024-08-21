using System;
using System.Reflection;
using EFS.ACommon;

namespace EfsML.NumericalRecipes
{
	#region Locate
	/// <summary>
	/// Given an array xx[0..n-1], and given a value x, returns a value j such that x is between xx[j-1]
	/// and xx[j]. xx must be monotonic, either increasing or decreasing. j=0 or j=n is returned
	/// to indicate that x is out of range.
	/// </summary>
	public class Locate
	{
		#region Members
		private int j;
		#endregion Members
		#region Accessors
		#region J (output)
		public int J {get{return j;}}
		#endregion J (output)
		#endregion Accessors
		#region Constructors
		public Locate(){}
		public Locate(double[] pXa, double pX)
		{
			Formula(pXa,pXa.Length,pX);
		}
		#endregion Constructors
		#region Methods
		#region Formula
		public void Formula(double[] pXa, int pN, double pX)
		{
            int ju   = pN + 1;
			int jl   = 0;
			bool ascnd = (pXa[pN - 1] > pXa[0]); 
			// Repeat until the test condition is satisfied
			while (1 < ju - jl) 
			{
                // Initialize lower and upper limits
                // If we are not yet done, compute a midpoint
                int jm = (ju + jl) >> 1;
                // replace either the lower limit or the upper limit as appropriate
                if (ascnd == (pXa[jm - 1] < pX))
					jl = jm;
				else
					ju = jm;
			}
			if (pX == pXa[0])
				j = 0;
			else if (pX == pXa[pXa.Length-1])
				j = pXa.Length - 1;
			else
				j = jl;
		}
		#endregion Formula
		#endregion Methods
	}
	#endregion Locate
	#region NearestPoints
	/// <summary>
	/// Return a vector of the most close points of a given point.
	/// </summary>
	public class NearestPoints
	{
		#region Members
		private double[,] nearestXa;
		private int       dimNearestXa;
		#endregion Members
		#region Accessors
		#region DimNearestXa
		public int DimNearestXa {get{return dimNearestXa;}}
		#endregion DimNearestXa
		#region NearestXa
		public double[,] NearestXa {get{return nearestXa;}}
		#endregion NearestXa
		#endregion Accessors
		#region Constructors
		public NearestPoints(){}
		public NearestPoints(double[,] pXa, double[] pX)
		{
			Formula(pXa,pX);
		}
		#endregion Constructors
		#region Methods
		#region Formula
		public void Formula(double[,] pXa,double[] pX)
		{
			#region recherche des 2^(dimension) points encadrant le point à valoriser
			int dim      = pXa.GetLength(1);
			double[,] x  = new double[2,dim];
			dimNearestXa = Convert.ToInt32(System.Math.Pow(2.0,dim));
			nearestXa    = new double[dimNearestXa,dim];
			// Alimentation des points pour interpolation
			for (int i=0;i<dimNearestXa;i++)
			{
				// valeur binaire du point à l'aide de l'indice
				int binaryPoint = Convert.ToInt32(Convert.ToString(i,2));
				for (int j=0;j<dim;j++)
				{
					// recherche du point le plus proche < au point à valoriser
					x[0,j]    = NearestMin(pXa,pX,j);
					// recherche du point le plus proche > au point à valoriser
					x[1,j]    = NearestMax(pXa,pX,j);
					if (Double.IsNaN(x[0,j]) || Double.IsNaN(x[1,j]))
					{
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
							"X{0} Direction out of matrix co-ordinates : {1}",j.ToString(),pX[j].ToString()); 
					}
					else if (0 < (binaryPoint & Convert.ToInt32(System.Math.Pow(2.0,dim-1-j))))
						nearestXa[i,j] = x[1,j];
					else
						nearestXa[i,j] = x[0,j];
				}
			}
			#endregion recherche des 2^(dimension) points encadrant le point à valoriser
		}
		#endregion Formula
		#region Nearest
		private static double Nearest(double[,] pXa, double[] pX, int pDim, string pNear)
		{
			double near_x = 0;
			double x      = pX[pDim];
            for (int i=0;i<pXa.GetLength(0);i++)
			{
                double xa = pXa[i, pDim];
                if ((0 == near_x) && (((x <= xa) && ("MAX" == pNear.ToUpper())) || ((x >= xa) && ("MIN" == pNear.ToUpper() ))))
					near_x = xa;
				else if ((x <= xa) && (pNear == "MAX"))
					near_x = System.Math.Min(near_x,xa);
				else if ((x >= xa) && (pNear == "MIN"))
					near_x = System.Math.Max(near_x,xa);
			}
			if (0 == near_x)
				near_x = Double.NaN;
			return near_x;
		}
		#endregion Nearest
		#region NearestMax
		private static double NearestMax(double[,] pXa,double[] pX,int pDim)
		{
			return Nearest(pXa,pX,pDim,"MAX");
		}
		#endregion NearestMax
		#region NearestMin
		private static double NearestMin(double[,] pXa,double[] pX,int pDim)
		{
			return Nearest(pXa,pX,pDim,"MIN");
		}
		#endregion NearestMin
		#region NearestX
		public double[] NearestX(int pPosition)
		{
			int dim           = nearestXa.GetLength(1);
			double[] nearestX = new double[dim];
			for (int i=0;i<dim;i++)
				nearestX[i] = nearestXa[pPosition,i];
			return nearestX;
		}
		#endregion NearestX
		#endregion Methods
	}
	#endregion NearestPoints
	#region Polin2
	/// <summary>
	/// Given arrays x1a[0..m-1] and x2a[0..n-1] of independent variables, and a submatrix of function
	/// values ya[0..m-1,0..n-1], tabulated at the grid points defined by x1a and x2a; and given values
	/// x1 and x2 of the independent variables; this routine returns an interpolated function value y,
	/// and an accuracy indication dy (based only on the interpolation in the x1 direction, however).
	/// </summary>
	public class Polin2
	{
		#region Members
		private double y;
		private double dy;
		#endregion Members
		#region Accessors
		#region Y (output)
		public double Y {get{return y;}}
		#endregion Y (output)
		#region Error (output)
		public double Error {get{return dy;}}
		#endregion Error (output)
		#endregion Accessors
		#region Constructors
		public Polin2(){}
		public Polin2(double[] pX1a, double[] pX2a, double[,] pYa, double pX1, double pX2)
		{
			Formula(pX1a,pX2a,pYa,pX1a.Length,pX2a.Length,pX1,pX2);
		}
		#endregion Constructors
		#region Methods
		#region Formula
		public void Formula(double[] pX1a, double[] pX2a, double[,] pYa, int pM, int pN, double pX1, double pX2)
		{
			double[] ymtmp = new Double[pM];
			double[] yntmp = new Double[pN];
			Polint polint  = new Polint();

			for (int j=0;j<pM;j++) 
			{
				for(int k=0;k<pN;k++)
					yntmp[k] = pYa[j,k];

				polint.Formula(pX2a,yntmp,pN,pX2);
				ymtmp[j] = polint.Y;
			}
			polint.Formula(pX1a,ymtmp,pM,pX1);
			y  = polint.Y;
			dy = polint.Error;
		}
		#endregion Formula
		#endregion Methods
	}
	#endregion Polin2
	#region Polint
	/// <summary>
	/// Given arrays pXa[0..n-1] and pYa[0..n-1], and given a value pX, this routine returns a value y, and
	/// an error estimate dy. If P(x) is the polynomial of degree N - 1 such that P(xai) = yai, i =
	/// 0, . . . , n-1, then the returned value y = P(x).
	/// </summary>
	public class Polint
	{
		#region Members
		private double y;
		private double dy;
		#endregion Members
		#region Accessors
		#region Y (output)
		public double Y {get{return y;}}
		#endregion Y (output)
		#region Error (output)
		public double Error {get{return dy;}}
		#endregion Error (output)
		#endregion Accessors
		#region Constructors
		public Polint(){}
		public Polint(double[] pXa, double[] pYa, double pX)
		{
			Formula(pXa,pYa,pXa.Length,pX);
		}
		#endregion Constructors
		#region Methods
		#region Formula
		public void Formula(double[] pXa, double[] pYa, int pN, double pX)
		{
            int ns = 0;
            double[] c  = new Double[pN];
			double[] d  = new Double[pN];

			double dif  = Math.Abs(pX - pXa[0]);
            int i;
            for (i = 0; i < pN; i++) // Here we find the index of the closest table entry,
            {
                double dift;
                if ((dift = Math.Abs(pX - pXa[i])) < dif)
                {
                    ns = i;
                    dif = dift;
                }
                c[i] = pYa[i];   // and initialize the tableau of c¡ and d¡.
                d[i] = pYa[i];
            }
            y = pYa[ns--];      // This is the initial approximation to y.
            int m;
            for (m = 0; m < pN - 1; m++) // For each column of the tableau,
            {
                for (i = 0; i < pN - m - 1; i++) // we loop over the current c¡ and d¡ and update them
                {
                    double ho = pXa[i] - pX;
                    double hp = pXa[i + m + 1] - pX;
                    double w = c[i + 1] - d[i];
                    double den;
                    if ((den = ho - hp) == 0.0)
                    {
                        // This error can occur only if two input xa¡ are (to within roundoff) identical.
                        // throw new SpheresException(MethodInfo.GetCurrentMethod().Name,"two input xa¡ are identical");
                        d[i] = 0;
                        c[i] = 0;
                        break;
                    }
                    den = w / den;
                    d[i] = hp * den;  // Here the c¡ and d¡ are updated.
                    c[i] = ho * den;
                }

                /*After each column in the tableau is completed, we decide which correction, c or d,
				we want to add to our accumulating value of y, i.e., which path to take through the
				tableau-forking up or down. We do this in such a way as to take the most "straight
				line" route through the tableau to its apex, updating ns accordingly to keep track of
				where we are. This route keeps the partial approximations centered (insofar as possible)
				on the target x. The last dy added is thus the error indication. */
                y += (dy = (2 * (ns + 1) < (pN - m - 1) ? c[ns + 1] : d[ns--]));
            }
        }
		#endregion Formula
		#endregion Methods
	}
	#endregion Polint
	#region Splie2
	/// <summary>
	/// Given an m by n tabulated function ya[0..m-1,0..n-1], and tabulated independent variables
	/// x2a[0..n-1], this routine constructs one-dimensional natural cubic splines of the rows of ya
	/// and returns the second-derivatives in the array y2a[0..m-1,0..n-1]. (The array x1a[0..m-1] is
	/// included in the argument list merely for consistency with routine splin2.)
	/// </summary>
	public class Splie2
	{
		#region Members
		private double[,] y2a;
		#endregion Members
		#region Accessors
		#region Y2a (output)
		public double[,] Y2a {get{return y2a;}}
		#endregion Y2a (output)
		#endregion Accessors
		#region Constructors
		public Splie2(){}
		public Splie2(double[] pX1a, double[] pX2a, double[,] pYa)
		{
			Formula(pX1a,pX2a,pYa,pX1a.Length,pX2a.Length);
		}
		#endregion Constructors
		#region Methods
		#region Formula
		public void Formula(double[] _, double[] pX2a, double[,] pYa, int pM, int pN)
		{
			double[] ytmp  = new Double[pN];
            y2a            = new double[pM,pN];
			Spline spline  = new Spline();
			for (int j=0;j<pM;j++)
			{
				for (int k=0;k<pN;k++) 
					ytmp[k] = pYa[j,k];
				spline.Formula(pX2a,ytmp,pN,1.0e30,1.0e30);
                double[] y2tmp = spline.Y2a;

                for (int k=0;k<pN;k++) 
					y2a[j,k] = y2tmp[k];
			}
		}
		#endregion Formula
		#endregion Methods
	}
	#endregion Splie2
	#region Splin2
	/// <summary>
	/// Given x1a, x2a, ya, m, n as described in splie2 and y2a as produced by that routine, and
	/// given a desired interpolating point x1,x2; this routine returns an interpolated function value y
	/// by bicubic spline interpolation.
	/// </summary>
	public class Splin2
	{
		#region Members
		private double y;
		#endregion Members
		#region Accessors
		#region Y (output)
		public double Y {get{return y;}}
		#endregion Y (output)
		#endregion Accessors
		#region Constructors
		public Splin2(){}
		public Splin2(double[] pX1a, double[] pX2a, double[,] pYa, double[,] pY2a, double pX1, double pX2)
			:this(pX1a,pX2a,pYa,pY2a,pX1a.Length,pX2a.Length,pX1,pX2){}

		public Splin2(double[] pX1a, double[] pX2a, double[,] pYa, double[,] pY2a, int pM, int pN, double pX1, double pX2)
		{
			Formula(pX1a,pX2a,pYa,pY2a,pM,pN,pX1,pX2);
		}
		#endregion Constructors
		#region Methods
		#region Formula
		public void Formula(double[] pX1a, double[] pX2a, double[,] pYa, double[,] pY2a, int pM, int pN, double pX1, double pX2)
		{
			double[] ytmp  = new Double[pN];
			double[] y2tmp = new Double[pN];
			double[] yytmp = new Double[pN];

			Spline spline = new Spline();
			Splint splint = new Splint();
			for (int j=0;j<pM;j++)
			{
				for(int k=0;k<pN;k++) 
				{
					ytmp[k]  = pYa[j,k];
					y2tmp[k] = pY2a[j,k];
				}
				splint.Formula(pX2a,ytmp,y2tmp,pN,pX2);
				yytmp[j] = splint.Y;
			}
			spline.Formula(pX1a,yytmp,pM,1.0e30,1.0e30);
			splint.Formula(pX1a,yytmp,spline.Y2a,pM,pX1);
			y = splint.Y;
		}
		#endregion Formula
		#endregion Methods
	}
	#endregion Splin2
	#region Spline
	/// <summary>
	/// Given arrays x[0..n-1] and y[0..n-1] containing a tabulated function, i.e., y = f(x), with
	/// x sorted, and given values yp_0 and yp_n-1 for the first derivative of the interpolating 
	/// function at points 0 and n-1, respectively, this routine returns an array y2[0..n-1] 
	/// that contains the second derivatives of the interpolating function at the 
	/// tabulated points x_i. If yp_0 and/or yp_n-1 are equal to 1 x 10^30 or larger, the routine is signaled 
	/// to set the corresponding boundary condition for a natural spline, with zero second derivative 
	/// on that boundary.
	/// </summary>
	public class Spline
	{
		#region Members
		private double[] y2a;
		#endregion Members
		#region Accessors
		#region Y2a (output)
		public double[] Y2a {get{return y2a;}}
		#endregion Y2a (output)
		#endregion Accessors
		#region Constructors
		public Spline(){}
		public Spline(double[] pXa, double[] pYa):this(pXa,pYa,1.0e30,1.0e30){}
		public Spline(double[] pXa, double[] pYa, double pYp1, double pYpn){Formula(pXa,pYa,pXa.Length,pYp1,pYpn);}
		#endregion Constructors
		#region Methods
		#region Formula
		public void Formula(double[] pXa, double[] pYa, int pN, double pYp1, double pYpn)
		{
			y2a        = new double[pN];
            double[] u = new Double[pN];

            if (0.99e30 < pYp1)
				y2a[0] = u[0] = 0.0;
			else 
			{
				y2a[0] = -0.5;
				u[0]   = (3.0 / (pXa[1] - pXa[0]) ) * ( (pXa[1] - pXa[0]) / (pXa[1] - pXa[0]) - pYp1);
			}
			for (int i=1;i<pN-1;i++) 
			{
                double sig = (pXa[i] - pXa[i - 1]) / (pXa[i + 1] - pXa[i - 1]);
                double p = sig * y2a[i - 1] + 2.0;
                y2a[i] = (sig - 1.0) / p;
				u[i]   = (pYa[i+1] - pYa[i]) / (pXa[i+1] - pXa[i]) - (pYa[i] - pYa[i-1]) / (pXa[i] - pXa[i-1]);
				u[i]   = (6.0 * u[i] / (pXa[i+1] - pXa[i-1]) - sig * u[i-1]) / p;
			}
            double un;
            double qn;
            if (0.99e30 < pYpn)
                qn = un = 0.0;
            else
            {
                qn = 0.5;
                un = (3.0 / (pXa[pN - 1] - pXa[pN - 2])) * (pYpn - (pYa[pN - 1] - pYa[pN - 2]) / (pXa[pN - 1] - pXa[pN - 2]));
            }
            y2a[pN-1] = (un - qn * u[pN-2]) / (qn * y2a[pN-2] + 1.0);
			for (int k=pN-2;k>=0;k--)
				y2a[k] = y2a[k] * y2a[k+1] + u[k];
		}
		#endregion Formula
		#endregion Methods
	}
	#endregion Spline
	#region Splint
	/// <summary>
	/// Given the arrays xa[0..n-1] and ya[0..n-1], which tabulate a function (with the xai¡ in order),
	/// and given the array y2a[0..n-1], which is the output from spline above, and given a value of
	/// x, this routine returns a cubic-spline interpolated value y.
	/// </summary>
	public class Splint
	{
		#region Members
		private double y;
		#endregion Members
		#region Accessors
		#region Y (output)
		public double Y {get{return y;}}
		#endregion Y (output)
		#endregion Accessors
		#region Constructors
		public Splint(){}
		public Splint(double[] pXa, double[] pYa, double[] pY2a, double pX)
		{
			Formula(pXa,pYa,pY2a,pXa.Length,pX);
		}
		#endregion Constructors
		#region Methods
		#region Formula
		public void Formula(double[] pXa, double[] pYa, double[] pY2a, int pN, double pX)
		{
            int klo = 0;
			int khi = pN - 1;
			double h;
			double b;
			double a;

			while (1 < khi - klo) 
			{
                int k = ((khi + klo + 2) >> 1) - 1;
                if (pX < pXa[k]) 
					khi = k;
				else 
					klo = k;
			}
			h = pXa[khi] - pXa[klo];
			if (h == 0.0)
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Bad xa input");
			a = (pXa[khi] - pX) / h;
			b = (pX - pXa[klo]) / h;
			y = a * pYa[klo] + b * pYa[khi] + ((a * a * a - a) * pY2a[klo] + (b * b * b - b) * pY2a[khi]) * (h * h) / 6.0;
		}
		#endregion Formula
		#endregion Methods
	}
	#endregion Splint
}
