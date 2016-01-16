using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonitorDarkener
{
    public partial class Form1 : Form
    {
        //List of all screens.
        Screen[] screens;

        //Primary screen
        Screen primary = null;

        #region CTOR & LOAD
        public Form1() { InitializeComponent(); }
        private void MainFormLoad(object sender, EventArgs e) { CheckDisplayCount(); }
        #endregion

        private void CheckDisplayCount()
        {
            //Make sure to init with a reset, incase this isn't the first time run. So we can remove buttons if the displays were changed.
            ResetDarkScreens();
            foreach (Button b in buttonPanel.Controls.OfType<Button>().ToList())
                if (b.Name != primaryDisplay.Name)
                    buttonPanel.Controls.Remove(b);

            //Check size for button, plus set offset.
            Size buttonSize = primaryDisplay.Size;
            Size buttonsOffset = new Size(25, 25);
            screens = Screen.AllScreens;

            //Find primary screen
            foreach (Screen s in screens)
                if (s.Primary)
                {
                    primary = s;
                    break;
                }

            foreach (Screen s in screens)
            {
                if (!s.Primary)
                {
                    //New button, set name to display to easier keep track of what button
                    //belongs to what display. Also disable Tab Stop, because we don't calculate where
                    //the button is relative to the others (pressing tab won't move selection in a logical manner).
                    Button newButton = new Button();
                    newButton.Name = string.Format("{0}_{1}", "Button", s.DeviceName);
                    newButton.TabStop = false;

                    bool isRight;
                    bool isDown;
                    Rectangle screenBounds = s.Bounds;

                    //Check other screen offset directions.
                    isRight = screenBounds.X > 0 ? true : false;
                    isDown = screenBounds.Y > 0 ? true : false;

                    //We only want to offset in X if the screen is offset in X.
                    decimal offsetRightAmount = 0;
                    if (screenBounds.X != 0)
                    {
                        offsetRightAmount = (decimal)screenBounds.Width / (decimal)primary.Bounds.Width;
                        offsetRightAmount *= isRight ? 1 : -1;
                        offsetRightAmount = RoundNearestInteger(offsetRightAmount);
                    }

                    //Same as above, except for Y.
                    decimal offsetDownAmount = 0;
                    if (screenBounds.Y != 0)
                    {
                        offsetDownAmount = (decimal)screenBounds.Width / (decimal)primary.Bounds.Width;
                        offsetDownAmount *= isDown ? 1 : -1;
                        offsetDownAmount = RoundNearestInteger(offsetDownAmount);
                    }

                    //Change button text to something somewhat logical.
                    string buttonText;
                    buttonText = string.Format("Screen: {2}{0} {1}", (int)offsetRightAmount, (int)offsetDownAmount, Environment.NewLine);
                    newButton.Text = buttonText;

                    //Move button on to the one displaying Primary.
                    newButton.Location = primaryDisplay.Location;

                    //Calculate offsets, we should only add an offset 
                    //if the screen is actually offset in said axis.
                    Size offset = new Size();
                    offset.Width = buttonSize.Width;
                    offset.Width *= (int)offsetRightAmount != 0 ? (int)offsetRightAmount : 0;
                    offset.Width += offsetRightAmount != 0 ? (offsetRightAmount > 0 ? 1 : -1) : 0;

                    offset.Height = buttonSize.Height;
                    offset.Height *= (int)offsetDownAmount != 0 ? (int)offsetDownAmount : 0;
                    offset.Height += offsetDownAmount != 0 ? (offsetDownAmount > 0 ? 1 : -1) : 0;

                    newButton.Size = buttonSize;
                    //Offset new button.
                    Point loc = Point.Add(newButton.Location, offset);
                    newButton.Location = loc;

                    newButton.Click += new System.EventHandler(DarkenScreen);
                    this.buttonPanel.Controls.Add(newButton);
                }
            }
        }

        /// <summary>
        /// Ceil or Floors a decimal depending on if it's negative. (Ceil if positive, Floor if negative).
        /// Prevents us from rounding it to 0.
        /// </summary>
        /// <param name="num">The number to ceil/floor to nearest integer (that's not 0)</param>
        /// <returns></returns>
        private decimal RoundNearestInteger(decimal num)
        {
            num = num > 0 ? Math.Floor(num) : Math.Ceiling(num);
            return num;
        }

        public void DarkenScreen(object sender, EventArgs e)
        {
            //Check if the color of our button is ControlDark.
            //If it is that means the button has been clicked already.
            //So make sure not to instantiate another dark screen.
            Button button = (Button)sender;
            if (button.BackColor != SystemColors.ControlDark)
            {
                foreach (Screen s in screens)
                    //Check if clicked button matches a certain screen.
                    //If it does, instantiate a new dark screen and scale/position it on that screen.
                    if (button.Name == "Button_" + s.DeviceName)
                    {
                        DarkForm newDarkForm = new DarkForm();
                        newDarkForm.Show();

                        newDarkForm.Location = new Point(s.Bounds.X, s.Bounds.Y);
                        newDarkForm.Size = new Size(s.Bounds.Width, s.Bounds.Height);
                        newDarkForm.Name = "Form_" + button.Name.Replace("Button_", "");

                        newDarkForm.Opacity = transparencySlider.Value / 100f;

                        SetButtonState(true, button);
                        break;
                    }
            }
            //If the button is dark, that means the button has been clicked,
            //so we should instead close the currently open dark screen.
            else
            {
                foreach (DarkForm d in Application.OpenForms.OfType<DarkForm>().ToList())
                    if (d.Name == "Form_" + button.Name.Replace("Button_", ""))
                    {
                        d.Close();
                        SetButtonState(false, button);
                    }
            }
        }

        /// <summary>
        /// Resets the buttons, and removes any dark screens currently visible.
        /// </summary>
        private void ResetDarkScreens()
        {
            foreach (DarkForm d in Application.OpenForms.OfType<DarkForm>().ToList())
            {
                d.Close();
            }
            foreach (Button b in buttonPanel.Controls)
            {
                if (b.Name != primaryDisplay.Name)
                {
                    SetButtonState(false, b);
                }
            }
        }

        /// <summary>
        /// Changes the state of a button
        /// </summary>
        /// <param name="clicked">Wether the button should be considered enabled(make a new dark screen) or remove the current one</param>
        /// <param name="button">The button we are changing</param>
        private void SetButtonState(bool clicked, Button button)
        {
            if (clicked)
            {
                button.BackColor = SystemColors.ControlDark;
            }
            else
            {
                button.BackColor = SystemColors.Control;
                button.UseVisualStyleBackColor = true;
            }
        }

        // If we change the transparency slider, update all visible dark screens.
        private void transparencyChanged(object sender, EventArgs e)
        {
            transparencyValueLabel.Text = string.Format("{0}%", transparencySlider.Value);
            foreach (DarkForm f in Application.OpenForms.OfType<DarkForm>().ToList())
            {
                f.Opacity = (transparencySlider.Value / 100f);
            }
        }

        #region One line methods. (Usually an event that simply invokes a method)
        private void resetButton_Click(object sender, EventArgs e) { ResetDarkScreens(); }
        private void refreshDisplaysToolStripMenuItem_Click(object sender, EventArgs e) { CheckDisplayCount(); }
        private void quitToolStripMenuItem_Click(object sender, EventArgs e) { this.Close(); }
        #endregion
    }
}
