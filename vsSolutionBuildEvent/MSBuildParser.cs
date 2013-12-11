﻿/* 
 * Boost Software License - Version 1.0 - August 17th, 2003
 * 
 * Copyright (c) 2013 Developed by reg <entry.reg@gmail.com>
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE80;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Collections;
using System.Text.RegularExpressions;

namespace reg.ext.vsSolutionBuildEvent
{
    class MSBuildParser: IMSBuildProperty, ISBEParserScript
    {

        /// <summary>
        /// MSBuild Property from default Project
        /// </summary>
        /// <param name="name">key property</param>
        /// <returns>evaluated value</returns>
        public string getProperty(string name)
        {
            return getProperty(name, null);
        }

        /// <summary>
        /// MSBuild Property from specific project
        /// </summary>
        /// <param name="name">key property</param>
        /// <param name="projectName">project name</param>
        /// <returns>evaluated value</returns>
        public string getProperty(string name, string projectName)
        {
            Project project         = getProject(projectName);
            ProjectProperty prop    = project.GetProperty(name);

            if(prop != null) {
                return prop.EvaluatedValue;
            }

            if(projectName == null) {
                projectName = "<default>";
            }
            throw new MSBuildParserProjectPropertyNotFoundException(String.Format("variable - '{0}' : project - '{1}'", name, projectName));
        }

        public List<MSBuildPropertyItem> listProperties(string projectName = null)
        {
            List<MSBuildPropertyItem> properties = new List<MSBuildPropertyItem>();

            Project project = getProject(projectName);
            foreach(ProjectProperty property in project.AllEvaluatedProperties) {
                properties.Add(new MSBuildPropertyItem(property.Name, property.EvaluatedValue));
            }
            return properties;
        }

        public List<string> listProjects()
        {
            List<string> projects = new List<string>();

            IEnumerator<Project> eprojects = loadedProjects();
            while(eprojects.MoveNext()) {
                string projectName = eprojects.Current.GetPropertyValue("ProjectName");
                if(projectName != null) {
                    if(!projects.Contains(projectName)) { //TODO: !
                        projects.Add(projectName);
                    }
                }
            }
            return projects;
        }

        /// <summary>
        /// handler to MSBuild environment variables (properties)
        /// </summary>
        /// <param name="data">text with $(ident) data</param>
        /// <returns>text with values of MSBuild properties</returns>
        public string parseVariablesMSBuild(string data)
        {
            return Regex.Replace(data, @"(?<!\$)\$\((?:([^\:\r\n\)]+?)\:([^\)\r\n]+?)|([^\)]*?))\)", delegate(Match m) {

                if(!m.Success) {
                    return m.Value;
                }

                // 3   -> $(name)
                // 1,2 -> $(name:project)

                if(m.Groups[3].Success) {
                    return getProperty(m.Groups[3].Value);
                }
                return getProperty(m.Groups[1].Value, m.Groups[2].Value);

            }, RegexOptions.IgnoreCase);
        }


        /// <summary>
        /// get default (first in the list at first time) project for access to properties etc.
        /// TODO: 
        /// </summary>
        /// <returns>Microsoft.Build.Evaluation.Project</returns>
        protected Project getProjectDefault()
        {
            IEnumerator<Project> eprojects = loadedProjects();
            if(eprojects.MoveNext()) {
                return eprojects.Current;
            }
            throw new MSBuildParserProjectNotFoundException("not found project: <default>");
        }

        protected Project getProject(string project)
        {
            if(project == null) {
                return getProjectDefault();
            }

            IEnumerator<Project> eprojects = loadedProjects();
            while(eprojects.MoveNext()) {
                if(eprojects.Current.GetPropertyValue("ProjectName").Equals(project)) {
                    return eprojects.Current;
                }
            }
            throw new MSBuildParserProjectNotFoundException(String.Format("not found project: '{0}'", project));
        }

        protected IEnumerator<Project> loadedProjects()
        {
            return ((ICollection<Project>)ProjectCollection.GlobalProjectCollection.LoadedProjects).GetEnumerator();
        }
    }

    /// <summary>
    /// item of property: name = value
    /// </summary>
    sealed class MSBuildPropertyItem
    {
        public string name;
        public string value;

        public MSBuildPropertyItem(string name, string value)
        {
            this.name  = name;
            this.value = value;
        }
    }
}