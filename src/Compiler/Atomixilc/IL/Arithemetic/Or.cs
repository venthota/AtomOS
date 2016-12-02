﻿using System;
using System.Collections.Generic;
using System.Reflection;

using Atomixilc.Machine;
using Atomixilc.Attributes;
using Atomixilc.Machine.x86;

namespace Atomixilc.IL
{
    [ILImpl(ILCode.Or)]
    internal class Or_il : MSIL
    {
        internal Or_il()
            : base(ILCode.Or)
        {

        }

        /*
         * URL : https://msdn.microsoft.com/en-us/library/system.reflection.emit.opcodes.Or(v=vs.110).aspx
         * Description : Compute the bitwise complement of the two integer values on top of the stack and pushes the result onto the evaluation stack.
         */
        internal override void Execute(Options Config, OpCodeType xOp, MethodBase method, Stack<StackItem> vStack)
        {
            if (vStack.Count < 2)
                throw new Exception("Internal Compiler Error: vStack.Count < 2");

            var itemA = vStack.Pop();
            var itemB = vStack.Pop();

            var size = Math.Max(Helper.GetTypeSize(itemA.OperandType, Config.TargetPlatform),
                Helper.GetTypeSize(itemB.OperandType, Config.TargetPlatform));

            /* The stack transitional behavior, in sequential order, is:
             * value1 is pushed onto the stack.
             * value2 is pushed onto the stack.
             * value2 and value1 are popped from the stack and their bitwise OR computed.
             * The result is pushed onto the stack.
             */

            switch (Config.TargetPlatform)
            {
                case Architecture.x86:
                    {
                        if (itemA.IsFloat || itemB.IsFloat || size > 4)
                            throw new Exception(string.Format("UnImplemented '{0}'", msIL));

                        if (itemB.RegisterOnly)
                            Swap(ref itemA, ref itemB);

                        if (itemB.RegisterOnly && itemB.RegisterRef == Register.EAX)
                            Swap(ref itemA, ref itemB);

                        Register DesReg;
                        if (itemA.RegisterOnly)
                        {
                            DesReg = itemA.RegisterRef.Value;
                        }
                        else
                        {
                            DesReg = Register.EAX;

                            new Mov
                            {
                                DestinationReg = Register.EAX,
                                SourceReg = itemA.RegisterRef,
                                SourceRef = itemA.AddressRef,
                                SourceIndirect = itemA.IsIndirect,
                                SourceDisplacement = itemA.Displacement
                            };
                        }

                        new Or
                        {
                            DestinationReg = DesReg,
                            SourceReg = itemB.RegisterRef,
                            SourceRef = itemB.AddressRef,
                            SourceIndirect = itemB.IsIndirect,
                            SourceDisplacement = itemB.Displacement
                        };

                        vStack.Push(new StackItem { RegisterRef = DesReg, OperandType = itemA.OperandType });
                    }
                    break;
                default:
                    throw new Exception(string.Format("Unsupported target platform '{0}' for MSIL '{1}'", Config.TargetPlatform, msIL));
            }
        }
    }
}