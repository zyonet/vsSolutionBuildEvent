﻿/*
 * Copyright (c) 2013 Developed by reg <entry.reg@gmail.com>
 * Distributed under the Boost Software License, Version 1.0
 * (See accompanying file LICENSE or copy at http://www.boost.org/LICENSE_1_0.txt)
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace reg.ext.vsSolutionBuildEvent
{
    public interface ITransferEnvironmentVariable
    {
        /// <summary>
        /// Output name of Environment Variable
        /// </summary>
        /// <param name="name"></param>
        /// <param name="project">name of project</param>
        void outputName(string name, string project);
    }

    public partial class EnvironmentVariablesFrm: Form
    {
        /// <summary>
        /// Support of output data
        /// </summary>
        private ITransferEnvironmentVariable _pin;

        /// <summary>
        /// Work with properties
        /// </summary>
        private MSBuildParser _msbuild;

        public EnvironmentVariablesFrm(ITransferEnvironmentVariable pin)
        {
            InitializeComponent();
            _msbuild    = new MSBuildParser();
            this._pin   = pin;
        }

        protected void fillProjects()
        {
            List<string> projects = _msbuild.listProjects();

            comboBoxProjects.Items.Clear();
            comboBoxProjects.Items.Add("<default>");
            comboBoxProjects.Items.AddRange(projects.ToArray());
            comboBoxProjects.SelectedIndex = 0;
        }

        protected void fillProperties(string project, string filter = null)
        {
            List<MSBuildPropertyItem> properties = _msbuild.listProperties(project);

            dataGridViewVariables.Rows.Clear();
            foreach(MSBuildPropertyItem prop in properties) {
                if(filter != null && !prop.name.ToLower().Contains(filter)) {
                    continue;
                }
                dataGridViewVariables.Rows.Add(prop.name, prop.value);
            }
        }

        protected void listRender()
        {
            fillProperties(getSelectedProject(), textBoxFilter.Text.Trim().ToLower());
            labelPropCount.Text = dataGridViewVariables.Rows.Count.ToString();
        }

        protected void keyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Escape) {
                this.Dispose();
                return;
            }

            if(e.KeyCode != Keys.Enter) {
                return;
            }

            foreach(DataGridViewRow row in dataGridViewVariables.Rows) {
                if(row.Selected) {
                    _pin.outputName(row.Cells[0].Value.ToString(), getSelectedProject());
                    this.Dispose();
                    return;
                }
            }
        }

        private string getSelectedProject()
        {
            if(comboBoxProjects.SelectedIndex > 0) {
                return comboBoxProjects.Text;
            }
            return null;
        }

        private void EnvironmentVariablesFrm_Load(object sender, EventArgs e)
        {
            fillProjects();            
            textBoxFilter.Select();
        }

        private void comboBoxProjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            listRender();
        }

        private void textBoxFilter_TextChanged(object sender, EventArgs e)
        {
            listRender();
        }

        private void textBoxFilter_KeyUp(object sender, KeyEventArgs e)
        {
            keyUp(sender, e);
        }

        private void dataGridViewVariables_KeyUp(object sender, KeyEventArgs e)
        {
            keyUp(sender, e);
        }

        private void dataGridViewVariables_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter) {
                e.SuppressKeyPress = true;
                return;
            }
        }

        private void comboBoxProjects_KeyUp(object sender, KeyEventArgs e)
        {
            keyUp(sender, e);
        }

        private void EnvironmentVariablesFrm_KeyUp(object sender, KeyEventArgs e)
        {
            keyUp(sender, e);
        }

        private void dataGridViewVariables_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex >= 0) {
                _pin.outputName(dataGridViewVariables[0, e.RowIndex].Value.ToString(), getSelectedProject());
                this.Dispose();
            }
        }
    }
}