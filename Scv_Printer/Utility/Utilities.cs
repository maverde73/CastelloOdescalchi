using System;
using System.Collections.Generic;
using System.Text;

namespace Thera.Biglietteria.Cassa
{
    public class Utilities
    {
        public enum TipoBiglietti
        {
            Scale,
            Ascensore,
            Ridotti,
            Gratuiti
        }

        public enum PrinterType
        {
            KPM300H,
            KubeLottery
        }

        public class PrinterState
        {
            public volatile Byte General = 0x00;
            public volatile Byte User = 0x00;
            public volatile Byte Recoverable = 0x00;
            public volatile Byte Unrecoverable = 0x00;

            public bool RetryError
            {
                get
                {
                    return gen_BlankPaper || usr_OpenCover || rec_JammedPaper;//|| gen_NotFoundNotch;
                }
            }

            public string RetryErrorMessage
            {
                get
                {
                    if (usr_OpenCover)
                    {
                        return "Coperchio aperto.Chiudere il coperchio";
                    }
                    else 
                    {
                       if (gen_BlankPaper)
                           return "Carta terminata.Inserire Carta";
                        else 
                         {
                             if (rec_JammedPaper)
                                 return "Carta incastrata. Liberare la stampante";
                             else
                             {
                                 if (gen_NotFoundNotch)
                                     return "Notch non trovato";
                             }

                        }

                       return "";
                    }
                }
            }

            #region General
            public bool  gen_BlankPaper
            {
                get
                {
                    return Convert.ToBoolean(General & (byte)GeneralState.enum_BlankPaper);
                }
            }            
            public bool gen_NearEndPaper
            {
                get
                {
                    return Convert.ToBoolean(General & (byte)GeneralState.enum_NearEndPaper);
                }
            }
            public bool gen_TicketPresent
            {
                get
                {
                    return Convert.ToBoolean(General & (byte)GeneralState.enum_TicketPresent);
                }
            }
            public bool gen_BlankVirtualPaper
            {
                get
                {
                    return Convert.ToBoolean(General & (byte)GeneralState.enum_BlankVirtualPaper);
                }
            }

            public bool gen_NotFoundNotch
            {
                get
                {
                    return Convert.ToBoolean(General & (byte)GeneralState.enum_NotFoundNotch);
                }
            }
            #endregion
            #region User            
            public bool usr_ErrorHead
            {
                get
                {
                    return Convert.ToBoolean(User & (byte)UserState.enum_ErrorHead);
                }
            }
            public bool usr_OpenCover
            {
                get
                {
                    return Convert.ToBoolean(User & (byte)UserState.enum_OpenCover);
                }
            }
            public bool usr_Spooling
            {
                get
                {
                    return Convert.ToBoolean(User & (byte)UserState.enum_Spooling);
                }
            }
            public bool usr_engineOn
            {
                get
                {
                    return Convert.ToBoolean(User & (byte)UserState.enum_engineOn);
                }
            }
            public bool usr_LFDown
            {
                get
                {
                    return Convert.ToBoolean(User & (byte)UserState.enum_LFDown);
                }
            }
            public bool usr_FFDown
            {
                get
                {
                    return Convert.ToBoolean(User & (byte)UserState.enum_FFDown);
                }
            }
            #endregion
            #region Recoverable
            public bool rec_TemperatureHeadError
            {
                get
                {
                    return Convert.ToBoolean(User & (byte)RecoverableError.enum_TemperatureHeadError);
                }
            }
            public bool rec_VoltageError
            {
                get
                {
                    return Convert.ToBoolean(User & (byte)RecoverableError.enum_VoltageError);
                }
            }
            public bool rec_CommandNotRecognizedError
            {
                get
                {
                    return Convert.ToBoolean(User & (byte)RecoverableError.enum_CommandNotRecognizedError);
                }
            }
            public bool rec_JammedPaper
            {
                get
                {
                    return Convert.ToBoolean(User & (byte)RecoverableError.enum_JammedPaper);
                }
            }
            public bool rec_NotchResearchError
            {
                get
                {
                    return Convert.ToBoolean(User & (byte)RecoverableError.enum_NotchResearchError);
                }
            }
            #endregion
            #region Unrecoverable
            public bool unrec_CutterError
            {
                get
                {
                    return Convert.ToBoolean(User & (byte)UnrecoverableError.enum_CutterError);
                }
            }
            public bool unrec_CutterCoverOpen
            {
                get
                {
                    return Convert.ToBoolean(User & (byte)UnrecoverableError.enum_CutterCoverOpen);
                }
            }
            public bool unrec_RAMErrorr
            {
                get
                {
                    return Convert.ToBoolean(User & (byte)UnrecoverableError.enum_RAMError);
                }
            }
            public bool unrec_EEPROMError
            {
                get
                {
                    return Convert.ToBoolean(User & (byte)UnrecoverableError.enum_EEPROMError);
                }
            }
            #endregion
        }
        [Flags]
        public enum GeneralState
        {
            enum_BlankPaper = 0x01,
            enum_Reserved1 = 0x02,
            enum_NearEndPaper = 0x04,
            enum_Reserved3 = 0x08,
            enum_Reserved4 = 0x10,
            enum_TicketPresent = 0x20,
            enum_BlankVirtualPaper = 0x40,
            enum_NotFoundNotch = 0x80
        }

        [Flags]
        public enum UserState
        {
            enum_ErrorHead = 0x01,
            enum_OpenCover = 0x02,
            enum_Spooling = 0x04,
            enum_engineOn = 0x08,
            enum_Reserved4 = 0x10,
            enum_LFDown = 0x20,
            enum_FFDown = 0x40,
            enum_Reserved7 = 0x80
        }

        [Flags]
        public enum RecoverableError
        {
            enum_TemperatureHeadError = 0x01,
            enum_ErrorCOMRS32 = 0x02,
            enum_Reserved2 = 0x04,
            enum_VoltageError = 0x08,
            enum_Reserved4 = 0x10,
            enum_CommandNotRecognizedError = 0x20,
            enum_JammedPaper = 0x40,
            enum_NotchResearchError = 0x80
        }

        [Flags]
        public enum UnrecoverableError
        {
            enum_CutterError = 0x01,
            enum_CutterCoverOpen = 0x02,
            enum_RAMError = 0x04,
            enum_EEPROMError = 0x08,
            enum_Reserved4 = 0x10,
            enum_Reserved5 = 0x20,
            enum_Reserved6 = 0x40,
            enum_Reserved7 = 0x80
        }
    }
   
}
