using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Makarov.Framework.Core.Managers
{

    public static class ProxyManager
    {
        #region Private members

        private static int _asmI;

        private static AssemblyName AsmName => new AssemblyName("MakarovProxyAssembly" + (++_asmI) + ".dll");

        private static AssemblyBuilder AsmBuilder => AssemblyBuilder.DefineDynamicAssembly(AsmName, AssemblyBuilderAccess.Run);

        private static ModuleBuilder ModBuilder => AsmBuilder.DefineDynamicModule(AsmName.Name);

        private static int _i;

        private static readonly Dictionary<string, Type> _cache = new Dictionary<string, Type>();

        #endregion

        #region Public methods

        public static object CreateProxy<TA>(object obj)
        {

            Type currType = obj.GetType();

            string currTypeS = currType.ToString();

            if (_cache.ContainsKey(currTypeS))
                return Activator.CreateInstance(_cache[currTypeS], new[] { obj });

            TypeBuilder typeBuilder = ModBuilder.DefineType(
                "ProxyObject_" + (_i++) + "_" + Guid.NewGuid(),
                TypeAttributes.Class | TypeAttributes.Public,
                typeof(object));

            FieldBuilder ownerField = typeBuilder.DefineField("_owner", currType, FieldAttributes.Private);

            foreach (var prop in currType.GetProperties())
                foreach (var attrib in prop.GetCustomAttributes(true))
                    if (attrib is TA)
                    {

                        PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(prop.Name,
                             PropertyAttributes.HasDefault,
                             prop.PropertyType,
                             null);

                        MethodBuilder methodGet = typeBuilder.DefineMethod("Get_" + prop.Name,
                                    MethodAttributes.Public,
                                    prop.PropertyType,
                                    new Type[] { });

                        ILGenerator ilGet = methodGet.GetILGenerator();

                        ilGet.Emit(OpCodes.Ldarg_0);
                        ilGet.Emit(OpCodes.Ldfld, ownerField);
                        ilGet.EmitCall(OpCodes.Callvirt, prop.GetGetMethod(), null);
                        ilGet.Emit(OpCodes.Ret);

                        MethodBuilder methodSet = typeBuilder.DefineMethod("Set_" + prop.Name,
                                    MethodAttributes.Public,
                                    null,
                                    new[] { prop.PropertyType });

                        ILGenerator ilSet = methodSet.GetILGenerator();

                        ilSet.Emit(OpCodes.Ldarg_0);
                        ilSet.Emit(OpCodes.Ldfld, ownerField);
                        ilSet.Emit(OpCodes.Ldarg_1);
                        ilSet.EmitCall(OpCodes.Callvirt, prop.GetSetMethod(), new[] { prop.PropertyType });
                        ilSet.Emit(OpCodes.Ret);

                        propertyBuilder.SetGetMethod(methodGet);
                        propertyBuilder.SetSetMethod(methodSet);

                        break;
                    }

            var ctorParams = new[] { currType };
            ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, ctorParams);
            ILGenerator ilgen = constructorBuilder.GetILGenerator();

            ilgen.Emit(OpCodes.Ldarg_0);
            ilgen.Emit(OpCodes.Call, typeof(object).GetConstructor(new Type[0]));

            ilgen.Emit(OpCodes.Ldarg_0);
            ilgen.Emit(OpCodes.Ldarg_1);
            ilgen.Emit(OpCodes.Stfld, ownerField);

            ilgen.Emit(OpCodes.Ret);

            Type type = typeBuilder.CreateType();
            _cache.Add(currTypeS, type);
            return Activator.CreateInstance(type, new[] { obj });
        }
        #endregion
    }
}
