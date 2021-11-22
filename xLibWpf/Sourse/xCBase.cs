using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using xLib.Sourse;

namespace xLib
{
    public unsafe class xCBaseCallback
    {
        public xCommand Command;
        public byte* Content;
        public int Size;
    }
    public class xCommand
    {
        public byte[] Key;
        public int ContentSize;
        public int Offset = 0;
        public xReceiverEvent Callback;
        public CBASE_MODE Mode = CBASE_MODE.OBJECT;
    }

    public enum CBASE_KEY : int { ACCEPT, CONTENT_ERROR }
    public enum CBASE_MODE : int { EVENT, OBJECT, CONTENT, DATA }

    public static class xCBase
    {        
        public static bool Add(List<xCommand> Commands, byte[] Key, xReceiverEvent Callback, int ContentSize, CBASE_MODE Mode)
        {
            bool accept = !(Commands == null || Key == null || Key.Length == 0);
            if (accept)
            {
                xCommand command = new xCommand();
                command.ContentSize = ContentSize;
                command.Callback = Callback;
                command.Mode = Mode;
                command.Key = Key;
                Commands.Add(command);
            }
            return accept;
        }

        public static unsafe bool Add(List<xCommand> Commands, xResponse Response, xReceiverEvent Callback)
        {
            if(Response == null) { return false; }
            Response.Command.Callback = Callback;
            Commands.Add(Response.Command);
            return true;
        }
        //===========================================================================================================================
        private static unsafe bool DataIdentification(xCommand Command, void *RxData, int RxDataLen)
        {
            bool accept = xConverter.Compare(Command.Key, RxData, Command.Key.Length);
            if (accept)
            {
                xCBaseCallback Packet = new xCBaseCallback();
                Packet.Command = Command;
                Packet.Size = RxDataLen;

                if (Command.ContentSize == Packet.Size) { Packet.Content = (byte*)RxData; Command.Callback(Packet, CBASE_KEY.ACCEPT); }
                else Command.Callback(Packet, CBASE_KEY.CONTENT_ERROR);

            }
            return accept;
        }
        //===========================================================================================================================
        private static unsafe bool ObjectIdentification(xCommand Command, void* RxData, int RxDataLen)
        {
            bool accept = xConverter.Compare(Command.Key, RxData, Command.Key.Length);            
            if (accept)
            {
                xCBaseCallback Packet = new xCBaseCallback();
                Packet.Command = Command;
                Packet.Size = RxDataLen - Command.Key.Length;

                if (Command.ContentSize == Packet.Size) { Packet.Content = (byte*)RxData + Command.Key.Length; Command.Callback(Packet, CBASE_KEY.ACCEPT); }
                else Command.Callback(Packet, CBASE_KEY.CONTENT_ERROR);
            }
            return accept;
        }
        //===========================================================================================================================
        private static unsafe bool EventIdentification(xCommand Command, void* RxData, int RxDataLen)
        {
            bool accept = xConverter.Compare(Command.Key, RxData, RxDataLen);
            if (accept) {
                xCBaseCallback Packet = new xCBaseCallback();
                Packet.Command = Command;
                Packet.Size = RxDataLen - Command.Key.Length;
                Packet.Content = (byte*)RxData;
                Command.Callback(Packet, CBASE_KEY.ACCEPT);
            }
            return accept;
        }
        //===========================================================================================================================
        private static unsafe bool ContentIdentification(xCommand Command, void* RxData, int RxDataLen)
        {
            bool accept = xConverter.Compare(Command.Key, RxData, Command.Key.Length - Command.Offset);
            if (accept)
            {
                xCBaseCallback Packet = new xCBaseCallback();
                Packet.Command = Command;
                Packet.Size = RxDataLen - Command.Key.Length;

                if (Packet.Size >= Command.ContentSize) { Packet.Content = (byte*)RxData + Command.Key.Length; Command.Callback(Packet, CBASE_KEY.ACCEPT); }
                else Command.Callback(Packet, CBASE_KEY.CONTENT_ERROR);
            }
            return accept;
        }
        //===========================================================================================================================
        public static unsafe bool Identification(List<xCommand> Commands, void *RxData, int RxDataLen)
        {
            if (RxData != null && RxDataLen > 0 && Commands.Count > 0)
            {
                for (int i = 0; i < Commands.Count; i++)
                {
                    switch (Commands[i].Mode)
                    {
                        case CBASE_MODE.OBJECT: if (ObjectIdentification(Commands[i], RxData, RxDataLen)) return true; break;
                        case CBASE_MODE.DATA: if (DataIdentification(Commands[i], RxData, RxDataLen)) return true; break;
                        case CBASE_MODE.EVENT: if (EventIdentification(Commands[i], RxData, RxDataLen)) return true; break;
                        case CBASE_MODE.CONTENT: if (ContentIdentification(Commands[i], RxData, RxDataLen)) return true; break;
                    }
                }
            }
            return false;
        }
        //===========================================================================================================================
        public static unsafe bool Identification(List<xCommand> Commands, byte[] RxData, int RxDataLen)
        {
            if (RxData != null && Commands != null && Commands.Count > 0) { fixed (byte* ptr = &RxData[0]) { return Identification(Commands, ptr, RxDataLen); } }
            return false;
        }
        //===========================================================================================================================
        
        public static unsafe int MemCopy(void* Out, void* In, int InSize, int Offset)
        {
            if (Out != null && In != null && InSize > 0)
            {
                byte* OutPtr = (byte*)Out;
                byte* InPtr = (byte*)In;
                for (int i = 0; i < InSize; i++) OutPtr[i + Offset] = InPtr[i];
                return InSize;
            }
            return 0;
        }

        public static unsafe int MemCopy(byte[] Out, byte[] In, int Offset) { if(Out != null && In != null) fixed(byte* ptr = Out) { return MemCopy(ptr, In, In.Length, Offset); } return 0; }
        public static unsafe int MemCopy(void* Out, void* In, int Size) { return MemCopy(Out, In, Size, 0); }
        public static unsafe int MemCopy(byte[] Out, void* In, int Size, int Offset) { if (Out != null && In != null) fixed (byte* ptr = Out) { return MemCopy(ptr, In, Size, Offset); } return 0; }

        public static unsafe int MemCopy(byte[] Out, string In, int Offset) {
            if (Out != null && In != null && In.Length > 0 && (Out.Length >= (In.Length + Offset)))
            {
                for (int i = 0; i < In.Length; i++) Out[i + Offset] = (byte)In[i];
                return In.Length;
            }
            return 0;
        }

        public static unsafe int MemCopy(void* Out, string Str, int Offset)
        {
            if (Out != null && Str != null && Str.Length > 0)
            {
                byte* OutPtr = (byte*)Out;
                for (int i = 0; i < Str.Length; i++) OutPtr[i + Offset] = (byte)Str[i];
                return Str.Length;
            }
            return 0;
        }

        public static unsafe int MemCopy(void* Out, byte[] In, int InSize, int Offset)
        {
            if (Out != null && In != null && InSize > 0)
            {
                byte* OutPtr = (byte*)Out;
                for (int i = 0; i < InSize; i++) OutPtr[i + Offset] = In[i];
                return InSize;
            }
            return 0;
        }

        public static unsafe bool memcmp(void* in_1, int offset_1, void *in_2, int offset_2, int count)
        {
            if (in_1 != null && in_2 != null && count > 0)
            {
                byte* ptr_1 = (byte*)in_1;
                byte* ptr_2 = (byte*)in_2;
                for (int i = 0; i < count; i++) { if (ptr_1[offset_1 + i] != ptr_2[offset_2 + i]) { return false; } }
                return true;
            }
            return false;
        }
    }
    //===============================================================================================================================================================================
    //===============================================================================================================================================================================
    public abstract class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
