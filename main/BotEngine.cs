using Sanderling;
using System;

public static class BotEngine
{
    public interface IMotor
    {
        public abstract void ActSequenceMotion(IEnumerable<Motion> seqMotion);
    }
    public enum MouseButtonIdEnum
    {
        None = 0x0,
        Left = 0x1,
        Middle = 0x2,
        Right = 0x3,
    }
    
    public class Motion : object
    {
        public readonly Vektor2DInt? MousePosition;
        public readonly MouseButtonIdEnum[] MouseButtonDown;
        public readonly MouseButtonIdEnum[] MouseButtonUp;
        public readonly WindowsInput.Native.VirtualKeyCode[] KeyDown;
        public readonly WindowsInput.Native.VirtualKeyCode[] KeyUp;
        public readonly string TextEntry;
        public readonly bool? WindowToForeground;

        public Motion(Vektor2DInt? mousePosition, MouseButtonIdEnum[] mouseButtonDown = null, MouseButtonIdEnum[] mouseButtonUp = null, WindowsInput.Native.VirtualKeyCode[] keyDown = null, WindowsInput.Native.VirtualKeyCode[] keyUp = null, string textEntry = null, System.Nullable<bool> windowToForeground = null)
        {
            MousePosition = mousePosition;
            MouseButtonDown = mouseButtonDown;
            MouseButtonUp = mouseButtonUp;
            KeyDown = keyDown;
            KeyUp = keyUp;
            TextEntry = textEntry;
            WindowToForeground = windowToForeground;
        }
    }
    public class MotionResult
    {
        public Int64 MotionRecommendationId;

        public bool Success;
    }

    static Int64[] ListeGrenzeAusÜberscnaidung1D(
            Int64 RegioonAScrankeMin,
            Int64 RegioonAScrankeMax,
            Int64 RegioonBScrankeMin,
            Int64 RegioonBScrankeMax)
    {
        var RegioonAMinMax = new KeyValuePair<Int64, Int64>(RegioonAScrankeMin, RegioonAScrankeMax);
        var RegioonBMinMax = new KeyValuePair<Int64, Int64>(RegioonBScrankeMin, RegioonBScrankeMax);

        var MengeKandidaat = new KeyValuePair<Int64, KeyValuePair<Int64, Int64>>[]{
                new KeyValuePair<Int64, KeyValuePair<Int64, Int64>>(RegioonAScrankeMin,RegioonBMinMax),
                new KeyValuePair<Int64, KeyValuePair<Int64, Int64>>(RegioonAScrankeMax,RegioonBMinMax),
                new KeyValuePair<Int64, KeyValuePair<Int64, Int64>>(RegioonBScrankeMin,RegioonAMinMax),
                new KeyValuePair<Int64, KeyValuePair<Int64, Int64>>(RegioonBScrankeMax,RegioonAMinMax),};

        var ListeGrenzePunkt =
            MengeKandidaat
            //	.Where((Kandidaat) => PunktLiigtInRegioon1D(Kandidaat.Value.Key, Kandidaat.Value.Value, Kandidaat.Key))
            .Where(Kandidaat => Kandidaat.Value.Key <= Kandidaat.Key && Kandidaat.Key <= Kandidaat.Value.Value)
            .Select((Kandidaat) => Kandidaat.Key)
            .ToArray();

        var ListeGrenzePunktDistinct =
            ListeGrenzePunkt.Distinct().ToArray();

        return ListeGrenzePunktDistinct;
    }
    static public IEnumerable<OrtogoonInt> Diferenz(
    this OrtogoonInt Minuend,
    OrtogoonInt Subtrahend)
    {
        if (null == Minuend)
        {
            yield break;
        }

        if (null == Subtrahend)
        {
            yield return Minuend;
            yield break;
        }

        var MinuendMinMax = new KeyValuePair<Vektor2DInt, Vektor2DInt>(Minuend.PunktMin, Minuend.PunktMax);
        var SubtrahendMinMax = new KeyValuePair<Vektor2DInt, Vektor2DInt>(Subtrahend.PunktMin, Subtrahend.PunktMax);

        if (MinuendMinMax.Value.A <= SubtrahendMinMax.Key.A ||
            MinuendMinMax.Value.B <= SubtrahendMinMax.Key.B ||
            SubtrahendMinMax.Value.A <= MinuendMinMax.Key.A ||
            SubtrahendMinMax.Value.B <= MinuendMinMax.Key.B)
        {
            //	Scpez Fal kaine Scnitmenge
            yield return Minuend;
            yield break;
        }

        if (MinuendMinMax.Value.A <= SubtrahendMinMax.Value.A &&
            MinuendMinMax.Value.B <= SubtrahendMinMax.Value.B &&
            SubtrahendMinMax.Key.A <= MinuendMinMax.Key.A &&
            SubtrahendMinMax.Key.B <= MinuendMinMax.Key.B)
        {
            //	Scpez Fal Minuend liigt volsctändig in Subtrahend
            yield break;
        }

        Int64[] RictungAMengeScranke =
            ListeGrenzeAusÜberscnaidung1D(
            MinuendMinMax.Key.A,
            MinuendMinMax.Value.A,
            SubtrahendMinMax.Key.A,
            SubtrahendMinMax.Value.A)
            .OrderBy((t) => t)
            .ToArray();

        Int64[] RictungBMengeScranke =
            ListeGrenzeAusÜberscnaidung1D(
            MinuendMinMax.Key.B,
            MinuendMinMax.Value.B,
            SubtrahendMinMax.Key.B,
            SubtrahendMinMax.Value.B)
            .OrderBy((t) => t)
            .ToArray();

        if (RictungAMengeScranke.Length < 1 || RictungBMengeScranke.Length < 1)
        {
            //	Scpez Fal kaine Scnitmenge,	(Redundant zur Prüüfung oobn)
            yield return Minuend;
            yield break;
        }

        var RictungAMengeScrankeMitMinuendGrenze =
            new Int64[] { MinuendMinMax.Key.A }.Concat(RictungAMengeScranke).Concat(new Int64[] { MinuendMinMax.Value.A }).ToArray();

        for (int RictungAScrankeIndex = 0; RictungAScrankeIndex < RictungAMengeScrankeMitMinuendGrenze.Length - 1; RictungAScrankeIndex++)
        {
            var RictungAScrankeMinLaage = RictungAMengeScrankeMitMinuendGrenze[RictungAScrankeIndex];

            var RictungAScrankeMaxLaage = RictungAMengeScrankeMitMinuendGrenze[RictungAScrankeIndex + 1];

            if (SubtrahendMinMax.Value.A <= RictungAScrankeMinLaage ||
                RictungAScrankeMaxLaage <= SubtrahendMinMax.Key.A)
            {
                //	in RictungB unbescrankter Abscnit
                yield return new OrtogoonInt(RictungAScrankeMinLaage, RictungAScrankeMaxLaage, MinuendMinMax.Key.B, MinuendMinMax.Value.B);
            }
            else
            {
                var RictungBMengeScrankeFrüheste = RictungBMengeScranke.First();
                var RictungBMengeScrankeLezte = RictungBMengeScranke.Last();

                if (MinuendMinMax.Key.B < SubtrahendMinMax.Key.B)
                {
                    yield return new OrtogoonInt(RictungAScrankeMinLaage, RictungAScrankeMaxLaage, MinuendMinMax.Key.B, SubtrahendMinMax.Key.B);
                }

                if (SubtrahendMinMax.Value.B < MinuendMinMax.Value.B)
                {
                    yield return new OrtogoonInt(RictungAScrankeMinLaage, RictungAScrankeMaxLaage, SubtrahendMinMax.Value.B, MinuendMinMax.Value.B);
                }
            }
        }
    }

    static public Nullable<T> CastToNullable<T>(
    this object Ref)
    where T : struct
    {
        if (Ref is T)
        {
            return (T)Ref;
        }

        return null;
    }
    public struct Vektor2DInt
    {
        public Vektor2DInt(Int64 A, Int64 B)
        {
            this.A = A;
            this.B = B;
        }

        public Vektor2DInt(Vektor2DInt ZuKopiirende)
            :
            this(ZuKopiirende.A, ZuKopiirende.B)
        {
        }

        public Int64 A;

        public Int64 B;

        public Vektor2DDouble AlsBib3Vektor2DDouble()
        {
            return new Vektor2DDouble(A, B);
        }

        public Vektor2DDouble Bib3Vektor2DDouble()
        {
            return new Vektor2DDouble(A, B);
        }

        static public Vektor2DInt operator -(Vektor2DInt Minuend, Vektor2DInt Subtrahend)
        {
            return new Vektor2DInt(Minuend.A - Subtrahend.A, Minuend.B - Subtrahend.B);
        }

        static public Vektor2DInt operator -(Vektor2DInt Subtrahend)
        {
            return new Vektor2DInt(0, 0) - Subtrahend;
        }

        static public Vektor2DInt operator +(Vektor2DInt Vektor0, Vektor2DInt Vektor1)
        {
            return new Vektor2DInt(Vektor0.A + Vektor1.A, Vektor0.B + Vektor1.B);
        }

        static public Vektor2DInt operator /(Vektor2DInt Dividend, Int64 Divisor)
        {
            return new Vektor2DInt((Dividend.A / Divisor), (Dividend.B / Divisor));
        }

        static public Vektor2DInt operator *(Vektor2DInt Vektor0, Int64 Faktor)
        {
            return new Vektor2DInt((Vektor0.A * Faktor), (Vektor0.B * Faktor));
        }

        static public Vektor2DInt operator *(Int64 Faktor, Vektor2DInt Vektor0)
        {
            return new Vektor2DInt((Vektor0.A * Faktor), (Vektor0.B * Faktor));
        }

        static public bool operator ==(Vektor2DInt Vektor0, Vektor2DInt Vektor1)
        {
            return Vektor0.A == Vektor1.A && Vektor0.B == Vektor1.B;
        }

        static public bool operator !=(Vektor2DInt Vektor0, Vektor2DInt Vektor1)
        {
            return !(Vektor0 == Vektor1);
        }

        public Int64 BetraagQuadriirt
        {
            get
            {
                return A * A + B * B;
            }
        }

        public Int64 Betraag
        {
            get
            {
                return (Int64)Math.Sqrt(BetraagQuadriirt);
            }
        }

        public double BetraagDouble
        {
            get
            {
                return Math.Sqrt(BetraagQuadriirt);
            }
        }

        public Vektor2DInt Normalisiirt()
        {
            var Betraag = this.Betraag;

            return new Vektor2DInt((this.A / Betraag), (this.B / Betraag));
        }

        static public double Skalarprodukt(Vektor2DInt vektor0, Vektor2DInt vektor1)
        {
            return vektor0.A * vektor1.A + vektor0.B * vektor1.B;
        }

        static System.Globalization.NumberFormatInfo KomponenteNumberFormat = KomponenteNumberFormatBerecne();

        static System.Globalization.NumberFormatInfo KomponenteNumberFormatBerecne()
        {
            var NumberFormat = (System.Globalization.NumberFormatInfo)System.Globalization.NumberFormatInfo.InvariantInfo.Clone();

            NumberFormat.NumberGroupSeparator = ".";
            NumberFormat.NumberGroupSizes = Enumerable.Range(0, 10).Select((t) => 3).ToArray();
            NumberFormat.NumberDecimalDigits = 0;

            return NumberFormat;
        }

        static public string KomponenteToString(Int64? Komponente)
        {
            if (!Komponente.HasValue)
            {
                return null;
            }

            return KomponenteToString(Komponente.Value);
        }

        static public string KomponenteToString(Int64 Komponente)
        {
            //	return Komponente.ToString("d", KomponenteNumberFormat);

            string Zwisceergeebnis = null;

            Int64 Rest = Math.Abs(Komponente);

            do
            {
                var NaacherRest = Rest / 1000;

                var Grupe = Rest % 1000;

                var GrupeString =
                    0 < NaacherRest ?
                    Grupe.ToString("D3") :
                    Grupe.ToString();

                Zwisceergeebnis =
                    null == Zwisceergeebnis ?
                    GrupeString :
                    GrupeString + " " + Zwisceergeebnis;

                Rest = NaacherRest;

            } while (0 < Rest);

            if (Komponente < 0)
            {
                return "-" + Zwisceergeebnis;
            }

            return Zwisceergeebnis;
        }

        override public string ToString()
        {
            return "A = " + KomponenteToString(A) + ", B = " + KomponenteToString(B);
        }

        public WinApi.Point AsWindowsPoint()
        {
            return new WinApi.Point((int)this.A, (int)this.B);
        }
    }
    public struct OrtogoonInt
    {
        public Int64 Min0;
        public Int64 Min1;
        public Int64 Max0;
        public Int64 Max1;

        public Vektor2DInt PunktMin
        {
            get
            {
                return new Vektor2DInt(Min0, Min1);
            }
        }

        public Vektor2DInt PunktMax
        {
            get
            {
                return new Vektor2DInt(Max0, Max1);
            }
        }

        public Int64 LängeRictung0
        {
            get
            {
                return Max0 - Min0;
            }
        }

        public Int64 LängeRictung1
        {
            get
            {
                return Max1 - Min1;
            }
        }

        public Vektor2DInt ZentrumLaage
        {
            get
            {
                return new Vektor2DInt((Min0 + Max0) / 2, (Min1 + Max1) / 2);
            }
        }

        public Vektor2DInt Grööse
        {
            get
            {
                return new Vektor2DInt(LängeRictung0, LängeRictung1);
            }
        }

        public IEnumerable<Vektor2DInt> ListeEkeLaage()
        {
            for (int EkeIndex = 0; EkeIndex < 4; EkeIndex++)
            {
                var RictungAGrenzeIndex = 1 == (((EkeIndex + 1) / 2) % 2);
                var RictungBGrenzeIndex = 1 == (((EkeIndex + 0) / 2) % 2);

                yield return new Vektor2DInt(RictungAGrenzeIndex ? Max0 : Min0, RictungBGrenzeIndex ? Max1 : Min1);
            }
        }

        static public OrtogoonInt Leer
        {
            get
            {
                return new OrtogoonInt(0, 0, 0, 0);
            }
        }

        static public OrtogoonInt LeerMin
        {
            get
            {
                return new OrtogoonInt(Int64.MinValue, Int64.MinValue, Int64.MinValue, Int64.MinValue);
            }
        }

        public OrtogoonInt(
            Int64 Min0,
            Int64 Min1,
            Int64 Max0,
            Int64 Max1)
        {
            this.Min0 = Min0;
            this.Min1 = Min1;
            this.Max0 = Max0;
            this.Max1 = Max1;
        }

        public OrtogoonInt(
            OrtogoonInt ZuKopiirende)
            :
            this(
            ZuKopiirende.Min0,
            ZuKopiirende.Min1,
            ZuKopiirende.Max0,
            ZuKopiirende.Max1)
        {
        }

        static public OrtogoonInt AusPunktMinUndPunktMax(
            Vektor2DInt PunktMinInkl,
            Vektor2DInt PunktMaxExkl)
        {
            return new OrtogoonInt(PunktMinInkl.A, PunktMinInkl.B, PunktMaxExkl.A, PunktMaxExkl.B);
        }

        static public OrtogoonInt AusPunktZentrumUndGrööse(
            Vektor2DInt ZentrumLaage,
            Vektor2DInt Grööse)
        {
            return OrtogoonInt.AusPunktMinUndPunktMax(
                (ZentrumLaage - Grööse / 2),
                (ZentrumLaage + ((Grööse + new Vektor2DInt(1, 1)) / 2)));
        }

        public OrtogoonInt Versezt(Vektor2DInt Vektor)
        {
            return new OrtogoonInt(
                Min0 + Vektor.A,
                Min1 + Vektor.B,
                Max0 + Vektor.A,
                Max1 + Vektor.B);
        }

        public OrtogoonInt VerseztAufZentrumLaage(Vektor2DInt ZentrumLaage)
        {
            var Versaz = ZentrumLaage - this.ZentrumLaage;

            return this.Versezt(Versaz);
        }

        public OrtogoonInt GrööseGeseztAngelpunktZentrum(Vektor2DInt Grööse)
        {
            return AusPunktZentrumUndGrööse(ZentrumLaage, Grööse);
        }

        static public OrtogoonInt Scnitfläce(OrtogoonInt O0, OrtogoonInt O1)
        {
            var ScnitfläceMin0 = Math.Min(O0.Max0, Math.Max(O0.Min0, O1.Min0));
            var ScnitfläceMin1 = Math.Min(O0.Max1, Math.Max(O0.Min1, O1.Min1));

            return new OrtogoonInt(
                ScnitfläceMin0,
                ScnitfläceMin1,
                Math.Max(ScnitfläceMin0, Math.Min(O0.Max0, O1.Max0)),
                Math.Max(ScnitfläceMin1, Math.Min(O0.Max1, O1.Max1)));
        }

        static public OrtogoonInt operator -(OrtogoonInt Minuend, Vektor2DInt Subtrahend)
        {
            return Minuend.Versezt(-Subtrahend);
        }

        static public OrtogoonInt operator +(OrtogoonInt Sumand0, Vektor2DInt Sumand1)
        {
            return Sumand0.Versezt(Sumand1);
        }

        static public OrtogoonInt operator *(OrtogoonInt Faktor0, Int64 Faktor1)
        {
            return new OrtogoonInt(Faktor0.Min0 * Faktor1, Faktor0.Min1 * Faktor1, Faktor0.Max0 * Faktor1, Faktor0.Max1 * Faktor1);
        }

        static public bool operator ==(OrtogoonInt O0, OrtogoonInt O1)
        {
            return
                O0.Min0 == O1.Min0 &&
                O0.Min1 == O1.Min1 &&
                O0.Max0 == O1.Max0 &&
                O0.Max1 == O1.Max1;
        }

        override public bool Equals(object Obj)
        {
            var AlsOrtogoon = CastToNullable<OrtogoonInt>(Obj);

            if (!AlsOrtogoon.HasValue)
            {
                return false;
            }

            return AlsOrtogoon.Value == this;
        }

        override public int GetHashCode()
        {
            return (Min0 + Min1 + Max0 + Max1).GetHashCode();
        }

        static public bool operator !=(OrtogoonInt Vektor0, OrtogoonInt Vektor1)
        {
            return !(Vektor0 == Vektor1);
        }

        public Int64 Betraag
        {
            get
            {
                return LängeRictung0 * LängeRictung1;
            }
        }

        public bool IsLeer
        {
            get
            {
                return 0 == Betraag;
            }
        }

        public bool EnthältPunktFürMinInklusiivUndMaxExklusiiv(Vektor2DInt Punkt)
        {
            return
                Min0 <= Punkt.A &&
                Min1 <= Punkt.B &&
                Punkt.A < Max0 &&
                Punkt.B < Max1;
        }

        public bool EnthältPunktFürMinInklusiivUndMaxInklusiiv(Vektor2DInt Punkt)
        {
            return
                Min0 <= Punkt.A &&
                Min1 <= Punkt.B &&
                Punkt.A <= Max0 &&
                Punkt.B <= Max1;
        }

        static public string KomponenteToString(Int64? Komponente)
        {
            return Vektor2DInt.KomponenteToString(Komponente);
        }

        override public string ToString()
        {
            return
                "Min0 = " + KomponenteToString(Min0) +
                ", Min1 = " + KomponenteToString(Min1) +
                ", Max0 = " + KomponenteToString(Max0) +
                ", Max1 = " + KomponenteToString(Max1);
        }
    }

    static public Vektor2DDouble NääxterPunktAufGeraadeSegment(
    Vektor2DDouble GeraadeSegmentBegin,
    Vektor2DDouble GeraadeSegmentEnde,
    Vektor2DDouble SuuceUrscprungPunktLaage,
    out double AufGeraadeNääxtePunktLaage)
    {
        var GeraadeSegmentLängeQuadraat = (GeraadeSegmentEnde - GeraadeSegmentBegin).BetraagQuadriirt;

        if (GeraadeSegmentLängeQuadraat <= 0)
        {
            if (SuuceUrscprungPunktLaage == GeraadeSegmentBegin)
            {
                AufGeraadeNääxtePunktLaage = 0;
            }
            else
            {
                AufGeraadeNääxtePunktLaage = double.PositiveInfinity;
            }

            return GeraadeSegmentBegin;
        }

        AufGeraadeNääxtePunktLaage =
            Vektor2DDouble.Skalarprodukt(
            SuuceUrscprungPunktLaage - GeraadeSegmentBegin,
            GeraadeSegmentEnde - GeraadeSegmentBegin) /
            GeraadeSegmentLängeQuadraat;

        if (AufGeraadeNääxtePunktLaage < 0)
        {
            return GeraadeSegmentBegin;
        }

        if (1 < AufGeraadeNääxtePunktLaage)
        {
            return GeraadeSegmentEnde;
        }

        return
            GeraadeSegmentBegin +
            AufGeraadeNääxtePunktLaage * (GeraadeSegmentEnde - GeraadeSegmentBegin);
    }


    static public Vektor2DDouble NääxterPunktAufGeraadeSegment(
    Vektor2DDouble GeraadeSegmentBegin,
    Vektor2DDouble GeraadeSegmentEnde,
    Vektor2DDouble SuuceUrscprungPunktLaage)
    {
        double AufGeraadeNääxtePunktLaage;

        return NääxterPunktAufGeraadeSegment(
            GeraadeSegmentBegin,
            GeraadeSegmentEnde,
            SuuceUrscprungPunktLaage,
            out AufGeraadeNääxtePunktLaage);
    }
    
    public struct Vektor2DDouble
    {
        public double A, B;

        public Vektor2DDouble(double A, double B)
        {
            this.A = A;
            this.B = B;
        }

        static public Vektor2DDouble operator -(Vektor2DDouble Minuend, Vektor2DDouble Subtrahend)
        {
            return new Vektor2DDouble(Minuend.A - Subtrahend.A, Minuend.B - Subtrahend.B);
        }

        static public Vektor2DDouble operator -(Vektor2DDouble Subtrahend)
        {
            return new Vektor2DDouble(0, 0) - Subtrahend;
        }

        static public Vektor2DDouble operator +(Vektor2DDouble Vektor0, Vektor2DDouble Vektor1)
        {
            return new Vektor2DDouble(Vektor0.A + Vektor1.A, Vektor0.B + Vektor1.B);
        }

        static public Vektor2DDouble operator /(Vektor2DDouble Dividend, double Divisor)
        {
            return new Vektor2DDouble(Dividend.A / Divisor, Dividend.B / Divisor);
        }

        static public Vektor2DDouble operator *(Vektor2DDouble Vektor0, double Faktor)
        {
            return new Vektor2DDouble(Vektor0.A * Faktor, Vektor0.B * Faktor);
        }

        static public Vektor2DDouble operator *(double Faktor, Vektor2DDouble Vektor0)
        {
            return new Vektor2DDouble(Vektor0.A * Faktor, Vektor0.B * Faktor);
        }

        static public bool operator ==(Vektor2DDouble Vektor0, Vektor2DDouble Vektor1)
        {
            return Vektor0.A == Vektor1.A && Vektor0.B == Vektor1.B;
        }

        static public bool operator !=(Vektor2DDouble Vektor0, Vektor2DDouble Vektor1)
        {
            return !(Vektor0 == Vektor1);
        }

        public double BetraagQuadriirt
        {
            get
            {
                return A * A + B * B;
            }
        }

        public double Betraag
        {
            get
            {
                return Math.Sqrt(BetraagQuadriirt);
            }
        }

        public Vektor2DDouble Normalisiirt()
        {
            var Betrag = this.Betraag;

            return new Vektor2DDouble(this.A / Betrag, this.B / Betrag);
        }

        public void Normalisiire()
        {
            var Length = this.Betraag;

            this.A = this.A / Length;
            this.B = this.B / Length;
        }

        static public double Skalarprodukt(
            Vektor2DDouble vektor0,
            Vektor2DDouble vektor1)
        {
            return
                vektor0.A * vektor1.A + vektor0.B * vektor1.B;
        }

        static public double Kroizprodukt(
            Vektor2DDouble vektor0,
            Vektor2DDouble vektor1)
        {
            return
                vektor0.A * vektor1.B - vektor0.B * vektor1.A;
        }

        override public string ToString()
        {
            return "{ A:" + A.ToString() + ", B:" + B.ToString() + "}";
        }
    }
    static public class Geometrik
    {
        static public bool ScnaidendGeraadeSegmentMitGeraadeSegment(
            Vektor2DDouble GeraadeSegment0Begin,
            Vektor2DDouble GeraadeSegment0Ende,
            Vektor2DDouble GeraadeSegment1Begin,
            Vektor2DDouble GeraadeSegment1Ende)
        {
            bool Paralel;
            bool Kolinear;
            bool Überlapend;

            if (ScnitpunktGeraadeSegmentMitGeraadeSegment(
                GeraadeSegment0Begin,
                GeraadeSegment0Ende,
                GeraadeSegment1Begin,
                GeraadeSegment1Ende,
                out Paralel,
                out Kolinear,
                out Überlapend).HasValue)
            {
                return true;
            }

            if (Überlapend)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
        /// </summary>
        /// <param name="GeraadeSegment0Begin"></param>
        /// <param name="GeraadeSegment0Ende"></param>
        /// <param name="GeraadeSegment1Begin"></param>
        /// <param name="GeraadeSegment1Ende"></param>
        /// <param name="Paralel"></param>
        /// <returns></returns>
        static public Vektor2DDouble? ScnitpunktGeraadeSegmentMitGeraadeSegment(
            Vektor2DDouble GeraadeSegment0Begin,
            Vektor2DDouble GeraadeSegment0Ende,
            Vektor2DDouble GeraadeSegment1Begin,
            Vektor2DDouble GeraadeSegment1Ende,
            out bool Paralel,
            out bool Kolinear,
            out bool Überlapend)
        {
            Kolinear = false;
            Überlapend = false;

            //	Suppose the two line segments run from p to p + r and from q to q + s
            var Segment0Vektor = GeraadeSegment0Ende - GeraadeSegment0Begin;
            var Segment1Vektor = GeraadeSegment1Ende - GeraadeSegment1Begin;

            var VektorKroizprodukt =
                Vektor2DDouble.Kroizprodukt(Segment0Vektor, Segment1Vektor);

            //	 Then any point on the first line is representable as p + t r (for a scalar parameter t)
            //	and any point on the second line as q + u s (for a scalar parameter u).

            //	t = (q − p) × s / (r × s)
            var ScnitpunktAufSegment0Antail =
                Vektor2DDouble.Kroizprodukt((GeraadeSegment1Begin - GeraadeSegment0Begin), Segment1Vektor) /
                VektorKroizprodukt;

            //	u = (q − p) × r / (r × s)
            var ScnitpunktAufSegment1Antail =
                Vektor2DDouble.Kroizprodukt((GeraadeSegment1Begin - GeraadeSegment0Begin), Segment0Vektor) /
                VektorKroizprodukt;

            if (0 == VektorKroizprodukt)
            {
                Paralel = true;

                if (0 == Vektor2DDouble.Kroizprodukt((GeraadeSegment1Begin - GeraadeSegment0Begin), Segment0Vektor))
                {
                    //	1.If r × s = 0 and (q − p) × r = 0, then the two lines are collinear.
                    Kolinear = true;

                    var Temp0 = Vektor2DDouble.Skalarprodukt(GeraadeSegment1Begin - GeraadeSegment0Begin, Segment0Vektor);
                    var Temp1 = Vektor2DDouble.Skalarprodukt(GeraadeSegment0Begin - GeraadeSegment1Begin, Segment1Vektor);

                    if (
                        0 <= Temp0 && Temp0 <= Vektor2DDouble.Skalarprodukt(Segment0Vektor, Segment0Vektor) ||
                        0 <= Temp1 && Temp1 <= Vektor2DDouble.Skalarprodukt(Segment1Vektor, Segment1Vektor))
                    {
                        //	If in addition, either 0 ≤ (q − p) · r ≤ r · r or 0 ≤ (p − q) · s ≤ s · s, then the two lines are overlapping.

                        Überlapend = true;
                        return null;
                    }
                }
            }
            else
            {
                Paralel = false;

                //	4.If r × s ≠ 0 and 0 ≤ t ≤ 1 and 0 ≤ u ≤ 1, the two line segments meet at the point p + t r = q + u s.
                if (0 <= ScnitpunktAufSegment0Antail && ScnitpunktAufSegment0Antail <= 1 &&
                    0 <= ScnitpunktAufSegment1Antail && ScnitpunktAufSegment1Antail <= 1)
                {
                    return GeraadeSegment0Begin + ScnitpunktAufSegment0Antail * Segment0Vektor;
                }
            }

            return null;
        }

        static public Vektor2DDouble NääxterPunktAufGeraadeSegment(
            Vektor2DDouble GeraadeSegmentBegin,
            Vektor2DDouble GeraadeSegmentEnde,
            Vektor2DDouble SuuceUrscprungPunktLaage)
        {
            double AufGeraadeNääxtePunktLaage;

            return NääxterPunktAufGeraadeSegment(
                GeraadeSegmentBegin,
                GeraadeSegmentEnde,
                SuuceUrscprungPunktLaage,
                out AufGeraadeNääxtePunktLaage);
        }

        static public Vektor2DDouble NääxterPunktAufGeraadeSegment(
            Vektor2DDouble GeraadeSegmentBegin,
            Vektor2DDouble GeraadeSegmentEnde,
            Vektor2DDouble SuuceUrscprungPunktLaage,
            out double AufGeraadeNääxtePunktLaage)
        {
            var GeraadeSegmentLängeQuadraat = (GeraadeSegmentEnde - GeraadeSegmentBegin).BetraagQuadriirt;

            if (GeraadeSegmentLängeQuadraat <= 0)
            {
                if (SuuceUrscprungPunktLaage == GeraadeSegmentBegin)
                {
                    AufGeraadeNääxtePunktLaage = 0;
                }
                else
                {
                    AufGeraadeNääxtePunktLaage = double.PositiveInfinity;
                }

                return GeraadeSegmentBegin;
            }

            AufGeraadeNääxtePunktLaage =
                Vektor2DDouble.Skalarprodukt(
                SuuceUrscprungPunktLaage - GeraadeSegmentBegin,
                GeraadeSegmentEnde - GeraadeSegmentBegin) /
                GeraadeSegmentLängeQuadraat;

            if (AufGeraadeNääxtePunktLaage < 0)
            {
                return GeraadeSegmentBegin;
            }

            if (1 < AufGeraadeNääxtePunktLaage)
            {
                return GeraadeSegmentEnde;
            }

            return
                GeraadeSegmentBegin +
                AufGeraadeNääxtePunktLaage * (GeraadeSegmentEnde - GeraadeSegmentBegin);
        }

        static public Vektor2DDouble NääxterPunktAufGeraade(
            Vektor2DDouble GeradeRichtung,
            Vektor2DDouble Punkt)
        {
            GeradeRichtung.Normalisiire();

            var PositionAufGerade = Punkt.A * GeradeRichtung.A + Punkt.B * GeradeRichtung.B;

            return new Vektor2DDouble(GeradeRichtung.A * PositionAufGerade, GeradeRichtung.B * PositionAufGerade);
        }

        static public Vektor2DDouble NääxterPunktAufGeraade(
            Vektor2DDouble GeradeRichtung,
            Vektor2DDouble Punkt,
            Vektor2DDouble GeradeVersatz)
        {
            return NääxterPunktAufGeraade(GeradeRichtung, Punkt - GeradeVersatz) + GeradeVersatz;
        }

        static public double DistanzVonPunktZuGeraade(
            Vektor2DDouble GeradeRichtung,
            Vektor2DDouble Punkt)
        {
            return (NääxterPunktAufGeraade(GeradeRichtung, Punkt) - Punkt).Betraag;
        }

        static public double DistanzVonPunktZuGeraadeSegment(
            Vektor2DDouble GeraadeSegmentBegin,
            Vektor2DDouble GeraadeSegmentEnde,
            Vektor2DDouble Punkt)
        {
            var AufGeraadeSegmentNääxterPunkt =
                NääxterPunktAufGeraadeSegment(GeraadeSegmentBegin, GeraadeSegmentEnde, Punkt);

            return (Punkt - AufGeraadeSegmentNääxterPunkt).Betraag;
        }

        /// <summary>
        /// berecnet aus der Menge der Punkte in der Folge aines Zyyklis wiiderhoolten Punkt den nääxten Punkt zu <paramref name="SuuceUrscprungPunktLaage"/>.
        /// </summary>
        /// <param name="SuuceUrscprungPunktLaage"></param>
        /// <param name="ZyyklusPunktVersaz"></param>
        /// <param name="ZyyklusLänge"></param>
        /// <returns></returns>
        static public double InFolgePunktNääxteBerecne(
            double SuuceUrscprungPunktLaage,
            double ZyyklusPunktLaage,
            double ZyyklusLänge)
        {
            var ZyyklusPunktLaageNääxteZuNul =
                ((((((ZyyklusPunktLaage / ZyyklusLänge) + 1) % 1) + 1.5) % 1) - 0.5) * ZyyklusLänge;

            var ZyyklusIndex = (Int64)((SuuceUrscprungPunktLaage - ZyyklusPunktLaageNääxteZuNul) / ZyyklusLänge + 0.5);

            return ZyyklusIndex * ZyyklusLänge + ZyyklusPunktLaageNääxteZuNul;
        }

        static public int[] KonvexeHüleListePunktIndexBerecne(
            Vektor2DDouble[] ListePunkt)
        {
            if (null == ListePunkt)
            {
                return null;
            }

            if (ListePunkt.Length < 4)
            {
                return Enumerable.Range(0, ListePunkt.Length).ToArray();
            }

            var MengePunktMitIndex =
                ListePunkt.Select((Punkt, Index) => new KeyValuePair<int, Vektor2DDouble>(Index, Punkt)).ToArray();

            var BeginPunktMitIndex =
                MengePunktMitIndex.OrderBy((Kandidaat) => Kandidaat.Value.A).FirstOrDefault();

            var HüleMengePunktIndex = new List<int>();

            HüleMengePunktIndex.Add(BeginPunktMitIndex.Key);

            var ZwisceergeebnisLeztePunktIndex = BeginPunktMitIndex.Key;
            var ZwisceergeebnisLeztePunktLaage = BeginPunktMitIndex.Value;
            double ZwisceergeebnisLezteRotatioon = 1.0 / 4;

            while (true)
            {
                var KandidaatNääxtePunktIndex = ZwisceergeebnisLeztePunktIndex;
                double KandidaatNääxteRotatioon = 1;

                for (int KandidaatPunktIndex = 0; KandidaatPunktIndex < ListePunkt.Length; KandidaatPunktIndex++)
                {
                    if (KandidaatPunktIndex == ZwisceergeebnisLeztePunktIndex)
                    {
                        continue;
                    }

                    var KandidaatPunkt = ListePunkt[KandidaatPunktIndex];

                    var KandidaatRotatioon =
                        (Rotatioon(KandidaatPunkt - ZwisceergeebnisLeztePunktLaage) -
                        ZwisceergeebnisLezteRotatioon + 1) % 1;

                    if (KandidaatNääxtePunktIndex == ZwisceergeebnisLeztePunktIndex ||
                        KandidaatRotatioon < KandidaatNääxteRotatioon)
                    {
                        KandidaatNääxteRotatioon = KandidaatRotatioon;
                        KandidaatNääxtePunktIndex = KandidaatPunktIndex;
                    }
                }

                if (KandidaatNääxtePunktIndex == BeginPunktMitIndex.Key)
                {
                    break;
                }

                HüleMengePunktIndex.Add(KandidaatNääxtePunktIndex);

                var TempZwisceergeebnisLeztePunktLaage = ListePunkt[KandidaatNääxtePunktIndex];

                ZwisceergeebnisLeztePunktIndex = KandidaatNääxtePunktIndex;
                ZwisceergeebnisLezteRotatioon = Rotatioon(TempZwisceergeebnisLeztePunktLaage - ZwisceergeebnisLeztePunktLaage);
                ZwisceergeebnisLeztePunktLaage = TempZwisceergeebnisLeztePunktLaage;
            }

            return HüleMengePunktIndex.ToArray();
        }

        /// <summary>
        /// Winkel(0) => (a=1,b=0)
        /// Winkel(1/4) => (a=0,b=1)
        /// Winkel(2/4) => (a=-1,b=0)
        /// Winkel(3/4) => (a=0,b=-1)
        /// </summary>
        /// <param name="Vektor"></param>
        static public double Rotatioon(Vektor2DDouble Vektor)
        {
            Vektor.Normalisiire();

            var Winkel = Math.Acos(Vektor.A) / Math.PI / 2;

            if (Vektor.B < 0)
            {
                Winkel = 1 - Winkel;
            }

            return Winkel;
        }

        static public double Rotatioon(
            Vektor2DDouble Vektor0,
            Vektor2DDouble Vektor1)
        {
            var Richtung0 = Vektor0.Normalisiirt();
            var Richtung1 = Vektor1.Normalisiirt();

            var Punktprodukt = Math.Min(1, Math.Max(-1, Vektor2DDouble.Skalarprodukt(Richtung0, Richtung1)));

            var Rotatioon = Math.Acos(Punktprodukt) / Math.PI / 2;

            return Rotatioon;
        }

        /// <summary>
        /// Links &lt; 0;
        /// Rechts &gt; 0;
        /// </summary>
        /// <param name="GeraadeRictung"></param>
        /// <param name="Punkt"></param>
        /// <returns></returns>
        static public int SaiteVonGeraadeZuPunkt(
            Vektor2DDouble GeraadeRictung,
            Vektor2DDouble Punkt)
        {
            return Math.Sign(Vektor2DDouble.Skalarprodukt(Punkt, new Vektor2DDouble(-GeraadeRictung.B, GeraadeRictung.A)));
        }

        static bool PunktLiigtInRegioon1D(
            Int64 RegioonMin,
            Int64 RegioonMax,
            Int64 Punkt)
        {
            return (RegioonMin <= Punkt && Punkt <= RegioonMax);
        }

        static Int64[] ListeGrenzeAusÜberscnaidung1D(
            Int64 RegioonAScrankeMin,
            Int64 RegioonAScrankeMax,
            Int64 RegioonBScrankeMin,
            Int64 RegioonBScrankeMax)
        {
            var RegioonAMinMax = new KeyValuePair<Int64, Int64>(RegioonAScrankeMin, RegioonAScrankeMax);
            var RegioonBMinMax = new KeyValuePair<Int64, Int64>(RegioonBScrankeMin, RegioonBScrankeMax);

            var MengeKandidaat = new KeyValuePair<Int64, KeyValuePair<Int64, Int64>>[]{
                new KeyValuePair<Int64, KeyValuePair<Int64, Int64>>(RegioonAScrankeMin,RegioonBMinMax),
                new KeyValuePair<Int64, KeyValuePair<Int64, Int64>>(RegioonAScrankeMax,RegioonBMinMax),
                new KeyValuePair<Int64, KeyValuePair<Int64, Int64>>(RegioonBScrankeMin,RegioonAMinMax),
                new KeyValuePair<Int64, KeyValuePair<Int64, Int64>>(RegioonBScrankeMax,RegioonAMinMax),};

            var ListeGrenzePunkt =
                MengeKandidaat
                //	.Where((Kandidaat) => PunktLiigtInRegioon1D(Kandidaat.Value.Key, Kandidaat.Value.Value, Kandidaat.Key))
                .Where(Kandidaat => Kandidaat.Value.Key <= Kandidaat.Key && Kandidaat.Key <= Kandidaat.Value.Value)
                .Select((Kandidaat) => Kandidaat.Key)
                .ToArray();

            var ListeGrenzePunktDistinct =
                ListeGrenzePunkt.Distinct().ToArray();

            return ListeGrenzePunktDistinct;
        }
    }
}
