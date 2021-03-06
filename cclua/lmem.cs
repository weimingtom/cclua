﻿using System;

using cc = cclua.lua530;

using lua_State = cclua.lua530.lua_State;

namespace cclua {

    public static partial class imp {

		public static long untracked_bytes = 0;

		public static long ccluaM_resetbytes () {
			long t = untracked_bytes;
			untracked_bytes = 0;
			return t;
		}

		public static void luaM_toobig (lua_State L) {
			luaG_runerror (L, "memory allocation error: block too big");
		}

		public static T luaM_newobject<T> (lua_State L) where T : new () {
            return new T ();
        }

        public static T[] luaM_emptyvector<T> (lua_State L, long n) {
            if (n <= 0)
                return null;
            return new T[n];
        }

        public static T[] luaM_fullvector<T> (lua_State L, long n) where T : new () {
            if (n <= 0)
                return null;
            T[] block = new T[n];
            for (long i = 0; i < n; i++)
                block[i] = new T ();
            return block;
        }

        public static T[] luaM_reallocv<T> (lua_State L, T[] block, long osize, long nsize) where T : new () {
			if (osize < 0 || nsize <= 0)  /* nsize==0 means free */
				return null;

            long realosize = (block != null) ? osize : 0;
			lua_assert ((realosize == 0) == (block == null));
            T[] newblock = luaM_emptyvector<T> (L, nsize);
			if (newblock == null && nsize > 0) {
				api_check (nsize > realosize, "realloc cannot fail when shrinking a block");
				luaD_throw (L, cc.LUA_ERRMEM);
			}
			lua_assert ((nsize == 0) == (newblock == null));
            long minsize = Math.Min (osize, nsize);
            for (long i = 0; i < minsize; i++) {
                newblock[i] = block[i];
            }
			return newblock;
		}

        public static void luaM_reallocvector<T> (lua_State L, ref T[] block, long osize, long nsize) where T : new () {
			block = luaM_reallocv<T> (L, block, osize, nsize);
		}

        public static void luaM_freearray (lua_State L, object block) {
            /* do nothing */
        }

        public static void luaM_free (lua_State L, object block) {
            /* do nothing */
        }


        public static void luaM_growvector<T> (lua_State L, ref T[] v, int nelems, int size, int limit, string e) where T : new () {
            if (nelems + 1 > size)
                v = luaM_growaux_<T> (L, v, ref size, limit, e);
        }


        public const int MINSIZEARRAY = 4;


        public static T[] luaM_growaux_<T> (lua_State L, T[] block, ref int size, int limit, string what) where T : new () {
            int newsize = 0;
            if (size >= limit / 2) {
                if (size >= limit)
                    luaG_runerror (L, "too many %s (limit is %d)", what, limit);
                newsize = limit;
            }
            else {
                newsize = size * 2;
                if (newsize < MINSIZEARRAY)
                    newsize = MINSIZEARRAY;
            }
            T[] newblock = luaM_reallocv<T> (L, block, size, newsize);
            return newblock;
        }
    }
}
