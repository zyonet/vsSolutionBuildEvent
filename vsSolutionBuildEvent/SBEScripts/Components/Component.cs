﻿/* 
 * Boost Software License - Version 1.0 - August 17th, 2003
 * 
 * Copyright (c) 2013-2014 Developed by reg [Denis Kuzmin] <entry.reg@gmail.com>
 * 
 * Permission is hereby granted, free of charge, to any person or organization
 * obtaining a copy of the software and accompanying documentation covered by
 * this license (the "Software") to use, reproduce, display, distribute,
 * execute, and transmit the Software, and to prepare derivative works of the
 * Software, and to permit third-parties to whom the Software is furnished to
 * do so, all subject to the following:
 * 
 * The copyright notices in the Software and this entire statement, including
 * the above license grant, this restriction and the following disclaimer,
 * must be included in all copies of the Software, in whole or in part, and
 * all derivative works of the Software, unless such copies or derivative
 * works are solely in the form of machine-executable object code generated by
 * a source language processor.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE, TITLE AND NON-INFRINGEMENT. IN NO EVENT
 * SHALL THE COPYRIGHT HOLDERS OR ANYONE DISTRIBUTING THE SOFTWARE BE LIABLE
 * FOR ANY DAMAGES OR OTHER LIABILITY, WHETHER IN CONTRACT, TORT OR OTHERWISE,
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE. 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using net.r_eg.vsSBE.MSBuild;

namespace net.r_eg.vsSBE.SBEScripts.Components
{
    public abstract class Component: IComponent
    {
        /// <summary>
        /// Ability to work with data for current component
        /// </summary>
        public abstract string Condition { get; }

        /// <summary>
        /// Handler for current data
        /// </summary>
        /// <param name="data">mixed data</param>
        /// <returns>prepared and evaluated data</returns>
        public abstract string parse(string data);

        /// <summary>
        /// Allows post-processing with MSBuild core.
        /// In general, some components can require immediate processing with evaluation, before passing control to next level
        /// </summary>
        public bool PostProcessingMSBuild
        {
            get { return postProcessingMSBuild; }
            set { postProcessingMSBuild = value; }
        }
        protected bool postProcessingMSBuild = false;

        /// <summary>
        /// Activation status
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }
        protected bool enabled = true;

        /// <summary>
        /// Sets location "as is" - after deepening
        /// </summary>
        public bool BeforeDeepen
        {
            get { return beforeDeepen; }
        }
        protected bool beforeDeepen = false;

        /// <summary>
        /// Disabled the forced post analysis
        /// </summary>
        public bool PostParse
        {
            get { return postParse; }
        }
        protected bool postParse = false;

        /// <summary>
        /// Disabled regex engine for property - condition
        /// </summary>
        bool IComponent.CRegex
        {
            get { return cregex; }
        }
        protected bool cregex = false;

        /// <summary>
        /// For evaluating with SBE-Script
        /// </summary>
        protected ISBEScript script;

        /// <summary>
        /// For evaluating with MSBuild
        /// </summary>
        protected IMSBuild msbuild;

        /// <summary>
        /// Provides operation with environment
        /// </summary>
        protected IEnvironment env;

        /// <summary>
        /// Current container of user-variables
        /// </summary>
        protected IUserVariable uvariable;

        /// <param name="env">Used environment</param>
        /// <param name="uvariable">Used instance of user-variables</param>
        public Component(IEnvironment env, IUserVariable uvariable): this()
        {
            this.env        = env;
            this.uvariable  = uvariable;
            script          = new Script(env, uvariable);
            msbuild         = new MSBuildParser(env, uvariable);
        }

        /// <param name="env">Used environment</param>
        public Component(IEnvironment env): this()
        {
            this.env = env;
        }

        public Component()
        {
            Log.nlog.Trace("init: '{0}'", this.ToString());
        }
    }
}
