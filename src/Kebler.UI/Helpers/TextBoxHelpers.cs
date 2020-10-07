using Kebler.Services;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kebler.UI.Helpers
{
    public class TextBoxHelpers
    {
        public enum TextBoxTypeEnum
        {
            Numeric,
            Decimal,
            Serial,
            Default
        }


        private static void targetTextbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
        }

        #region IsNumericProperty

        public static bool GetIsNumeric(DependencyObject obj)
        {
            return (bool) obj.GetValue(IsNumericProperty);
        }

        public static void SetIsNumeric(DependencyObject obj, bool value)
        {
            obj.SetValue(IsNumericProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsNumeric.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsNumericProperty =
            DependencyProperty.RegisterAttached("IsNumeric", typeof(bool), typeof(TextBoxHelpers), new PropertyMetadata(
                false, (s, e) =>
                {
                    var targetTextbox = s as TextBox;
                    if (targetTextbox != null)
                    {
                        if ((bool) e.OldValue && !(bool) e.NewValue)
                        {
                            targetTextbox.PreviewTextInput -= targetTextbox_PreviewTextInputNum;
                            targetTextbox.PreviewKeyDown -= targetTextbox_PreviewKeyDown;
                            targetTextbox.TextChanged -= targetTextboxTextChanged;
                        }

                        if ((bool) e.NewValue)
                        {
                            targetTextbox.PreviewTextInput += targetTextbox_PreviewTextInputNum;
                            targetTextbox.PreviewKeyDown += targetTextbox_PreviewKeyDown;
                            //Это для лимитов
                            targetTextbox.TextChanged += targetTextboxTextChanged;

                            //Если установлено MinValue, то вписываем его.
                            var minvalue = GetMinValue(targetTextbox);
                            if (minvalue != null)
                                targetTextbox.Text = Convert.ToInt32(Math.Truncate((decimal) minvalue))
                                    .ToString(CultureInfo.InvariantCulture);
                        }
                    }
                }));

        private static void targetTextbox_PreviewTextInputNum(object sender, TextCompositionEventArgs e)
        {
            var newChar = e.Text[0];
            e.Handled = !char.IsNumber(newChar);
            if (e.Handled)
            {
                var targetTextbox = (TextBox) sender;
                var text = targetTextbox.Text;

                var minvalue = GetMinValue(targetTextbox);
                if (minvalue != null && text.IsNullOrEmpty())
                    targetTextbox.Text = Convert.ToInt32(Math.Truncate((decimal) minvalue))
                        .ToString(CultureInfo.InvariantCulture);

                /*int maxvalue = int.MaxValue;
                if ((GetMaxValue(targetTextbox) >= int.MinValue) && (GetMaxValue(targetTextbox) <= int.MaxValue))
                {
                    maxvalue = Convert.ToInt32(Math.Truncate(GetMaxValue(targetTextbox)));
                }

                if (!text.IsNullOrEmpty())
                {
                    var num = Convert.ToInt32(text, CultureInfo.InvariantCulture);
                    if (num < minvalue)
                    {
                        targetTextbox.Text = minvalue.ToString(CultureInfo.InvariantCulture);
                    }
                    if (num > maxvalue)
                    {
                        targetTextbox.Text = maxvalue.ToString(CultureInfo.InvariantCulture);
                    }
                }*/
            }
        }

        #endregion IsNumericProperty

        #region IsDecimalProperty

        public static bool GetIsDecimal(DependencyObject obj)
        {
            return (bool) obj.GetValue(IsDecimalProperty);
        }

        public static void SetIsDecimal(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDecimalProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsNumeric.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDecimalProperty =
            DependencyProperty.RegisterAttached("IsDecimal", typeof(bool), typeof(TextBoxHelpers), new PropertyMetadata(
                false, (s, e) =>
                {
                    var targetTextbox = s as TextBox;
                    if (targetTextbox != null)
                    {
                        if ((bool) e.OldValue && !(bool) e.NewValue)
                        {
                            targetTextbox.PreviewTextInput -= targetTextbox_PreviewTextInputDec;
                            targetTextbox.PreviewKeyDown -= targetTextbox_PreviewKeyDown;
                            targetTextbox.TextChanged -= targetTextboxTextChanged;
                        }

                        if ((bool) e.NewValue)
                        {
                            targetTextbox.PreviewTextInput += targetTextbox_PreviewTextInputDec;
                            targetTextbox.PreviewKeyDown += targetTextbox_PreviewKeyDown;
                            //Это для лимитов
                            targetTextbox.TextChanged += targetTextboxTextChanged;


                            //Если установлено MinValue, то вписываем его.
                            var minvalue = GetMinValue(targetTextbox);
                            if (minvalue != null)
                                targetTextbox.Text = ((decimal) minvalue).ToString(CultureInfo.InvariantCulture);
                        }
                    }
                }));

        private static void targetTextbox_PreviewTextInputDec(object sender, TextCompositionEventArgs e)
        {
            var newChar = e.Text[0];
            var a = Convert.ToChar(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);

            var isNumber = char.IsNumber(newChar);
            var isSeparator = newChar == a;
            var tb = sender as TextBox;
            var isSeparatorContains = true;
            if (tb != null && isSeparator)
                isSeparatorContains = tb.Text.Contains(newChar.ToString());

            if (isNumber)
                e.Handled = false;
            else
                e.Handled = !isSeparator || isSeparatorContains;

            //Не корректный ввод
            if (e.Handled)
            {
                var targetTextbox = (TextBox) sender;
                var text = targetTextbox.Text;

                var minvalue = GetMinValue(targetTextbox);
                if (minvalue != null && text.IsNullOrEmpty())
                    targetTextbox.Text = ((decimal) minvalue).ToString(CultureInfo.InvariantCulture);

                /*decimal maxvalue = GetMaxValue(targetTextbox);
                if (!text.IsNullOrEmpty())
                {
                    var num = Convert.ToDecimal(text, CultureInfo.InvariantCulture);
                    if (num < minvalue)
                    {
                        targetTextbox.Text = minvalue.ToString(CultureInfo.InvariantCulture);
                    }
                    if (num > maxvalue)
                    {
                        targetTextbox.Text = maxvalue.ToString(CultureInfo.InvariantCulture);
                    }
                }*/
            }
        }

        #endregion IsDecimalProperty

        //работает для IsDecimal и IsNumeric

        #region MinMaxProperty

        public static decimal? GetMinValue(DependencyObject obj)
        {
            return (decimal?) obj.GetValue(MinValue);
        }

        public static void SetMinValue(DependencyObject obj, decimal? value)
        {
            obj.SetValue(MinValue, value);
        }

        //Using a DependencyProperty as the backing store for IsNumeric.This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinValue =
            DependencyProperty.RegisterAttached("MinValue", typeof(decimal?), typeof(TextBoxHelpers));

        /*public static readonly DependencyProperty MinValue =
			DependencyProperty.RegisterAttached("MinValue", typeof(decimal), typeof(TextBoxHelpers), new PropertyMetadata(decimal.MinValue, (s, e) =>
			{
                var targetTextbox = s as TextBox;
				if (targetTextbox != null)
                {
                    //Для decimal и numeric
                    //if ((GetIsDecimal(targetTextbox) || GetIsNumeric(targetTextbox)) && (decimal)e.NewValue != decimal.MaxValue)
                    //{
                    //    //targetTextbox.LostFocus += targetTextboxOnLostFocus;
                    //    targetTextbox.TextChanged += targetTextboxTextChanged;
                    //}
                }
            }));*/

        public static decimal? GetMaxValue(DependencyObject obj)
        {
            return (decimal?) obj.GetValue(MaxValue);
        }

        public static void SetMaxValue(DependencyObject obj, decimal? value)
        {
            obj.SetValue(MaxValue, value);
        }

        //Using a DependencyProperty as the backing store for IsNumeric.This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxValue =
            DependencyProperty.RegisterAttached("MaxValue", typeof(decimal?), typeof(TextBoxHelpers));

        /*public static readonly DependencyProperty MaxValue =
			DependencyProperty.RegisterAttached("MaxValue", typeof(decimal), typeof(TextBoxHelpers), new PropertyMetadata(decimal.MaxValue, (s, e) =>
			{
				var targetTextbox = s as TextBox;
				if (targetTextbox != null)
				{
                    //Для decimal и numeric
                    //if ((GetIsDecimal(targetTextbox) || GetIsNumeric(targetTextbox)) && (decimal)e.NewValue != decimal.MaxValue)
                    //{
                    //    //targetTextbox.LostFocus += targetTextboxOnLostFocus;
                    //    targetTextbox.TextChanged += targetTextboxTextChanged;
                    //}

				}
			}));*/

        private static void targetTextboxTextChanged(object sender, TextChangedEventArgs e)
        {
            var targetTextbox = (TextBox) sender;

            var text = targetTextbox.Text;
            if (text.IsNullOrEmpty()) text = "0";

            decimal num = 0; // = null?
            decimal.TryParse(text, out num);

            //var num = Convert.ToDecimal(text, CultureInfo.InvariantCulture);            
            var minvalue = GetMinValue(targetTextbox);
            var maxvalue = GetMaxValue(targetTextbox);

            //Decimal
            if (GetIsDecimal(targetTextbox))
            {
                if (minvalue != null && num < minvalue)
                    targetTextbox.Text = ((decimal) minvalue).ToString(CultureInfo.InvariantCulture);
                if (maxvalue != null && num > maxvalue)
                    targetTextbox.Text = ((decimal) maxvalue).ToString(CultureInfo.InvariantCulture);
            }

            //Numeric
            if (GetIsNumeric(targetTextbox))
            {
                //int minValueInt = int.MinValue;
                //int maxValueInt = int.MaxValue;

                if (minvalue != null && num < minvalue)
                    targetTextbox.Text = Convert.ToInt32(Math.Truncate((decimal) minvalue))
                        .ToString(CultureInfo.InvariantCulture);
                if (maxvalue != null && num > maxvalue)
                    targetTextbox.Text = Convert.ToInt32(Math.Truncate((decimal) maxvalue))
                        .ToString(CultureInfo.InvariantCulture);
            }
        }

        //private static void targetTextboxOnLostFocus(object sender, RoutedEventArgs e)
        //{
        //	var targetTextbox = (TextBox)sender;
        //	var minvalue = GetMinValue(targetTextbox).ToString(CultureInfo.InvariantCulture);
        //	var maxvalue = GetMaxValue(targetTextbox).ToString(CultureInfo.InvariantCulture);

        //	if (targetTextbox.Text.IsNullOrEmpty())
        //		targetTextbox.Text = minvalue;
        //	var text = targetTextbox.Text;
        //	var num = Convert.ToDecimal(text, CultureInfo.InvariantCulture);
        //	if (num < GetMinValue(targetTextbox))
        //	{
        //		targetTextbox.Text = minvalue;
        //	}
        //	if (num > GetMaxValue(targetTextbox))
        //	{
        //		targetTextbox.Text = maxvalue;
        //	}
        //}

        #endregion MinMaxProperty

        #region IsSerialProperty

        public static bool GetIsSerial(DependencyObject obj)
        {
            return (bool) obj.GetValue(IsSerialProperty);
        }

        public static void SetIsSerial(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSerialProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsNumeric.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSerialProperty =
            DependencyProperty.RegisterAttached("IsSerial", typeof(bool), typeof(TextBoxHelpers), new PropertyMetadata(
                false, (s, e) =>
                {
                    var targetTextbox = s as TextBox;
                    if (targetTextbox != null)
                    {
                        if ((bool) e.OldValue && !(bool) e.NewValue)
                            targetTextbox.PreviewTextInput -= targetTextbox_PreviewTextInputSerial;
                        if ((bool) e.NewValue)
                        {
                            targetTextbox.PreviewTextInput += targetTextbox_PreviewTextInputSerial;
                            targetTextbox.PreviewKeyDown += targetTextbox_PreviewKeyDown;
                        }
                    }
                }));

        private static void targetTextbox_PreviewTextInputSerial(object sender, TextCompositionEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Text))
            {
                var newChar = e.Text.ToUpper()[0];
                e.Handled = !Allowed_Char(newChar);
            }
            else
            {
                e.Handled = true;
            }

            //e.Handled = !Char.IsNumber(newChar); 
        }

        /// <summary>
        ///     Проверка на ввод разрешенных символов
        /// </summary>
        /// <param name="ss"></param>
        /// <returns></returns>
        public static bool Allowed_Char(char ss)
        {
            var allowed = false;
            if (ss >= 'A' && ss <= 'Z' && ss != 'O' || ss >= '0' && ss <= '9' || ss == '-')
                allowed = true;
            return allowed;
        }

        #endregion IsSerialProperty

        #region IsRuLattersProperty

        public static bool GetIsRuLatters(DependencyObject obj)
        {
            return (bool) obj.GetValue(IsRuLattersProperty);
        }

        public static void SetIsRuLatters(DependencyObject obj, bool value)
        {
            obj.SetValue(IsRuLattersProperty, value);
        }

        public static readonly DependencyProperty IsRuLattersProperty = DependencyProperty.RegisterAttached(
            "IsRuLatters", typeof(bool), typeof(TextBoxHelpers), new PropertyMetadata(false, (s, e) =>
            {
                var textBoxRuLatters = s as TextBox;
                if (textBoxRuLatters != null)
                {
                    if ((bool) e.OldValue && !(bool) e.NewValue)
                        textBoxRuLatters.PreviewTextInput -= TextBoxRuLatters_PreviewTextInput;
                    if ((bool) e.NewValue)
                        textBoxRuLatters.PreviewTextInput += TextBoxRuLatters_PreviewTextInput;
                    //textBoxRuLatters.PreviewKeyDown += TextBoxRuLatters_PreviewKeyDown;
                }
            }));

        private static void TextBoxRuLatters_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Text))
            {
                var newChar = e.Text[0];
                e.Handled = !DisableEn_Char(newChar);
            }
            else
            {
                e.Handled = true;
            }
        }

        //private static void TextBoxRuLatters_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    e.Handled = e.Key == Key.Space;
        //}

        /// <summary>
        ///     Проверка на ввод разрешенных русских символов
        /// </summary>
        /// <param name="ss"></param>
        /// <returns></returns>
        public static bool DisableEn_Char(char ss)
        {
            var allowed = true;
            if (ss >= 'A' && ss <= 'z')
                allowed = false;
            return allowed;
        }

        #endregion


        #region IsFocusedProperty

        public static readonly DependencyProperty IsFocusedProperty
            = DependencyProperty.RegisterAttached("IsFocused", typeof(bool), typeof(TextBoxHelpers),
                new PropertyMetadata(false, OnIsFocusedChanged));

        public static bool GetIsFocused(DependencyObject dependencyObject)
        {
            return (bool) dependencyObject.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(IsFocusedProperty, value);
        }

        public static void OnIsFocusedChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var textBox = dependencyObject as TextBox;
            if (null == textBox)
                return;

            var newValue = (bool) dependencyPropertyChangedEventArgs.NewValue;
            if (newValue /*&& !oldValue*/ && !textBox.IsFocused)
                textBox.Focus();
        }

        #endregion IsFocusedProperty


        #region TypeFormatProperty

        public static readonly DependencyProperty TypeFormatProperty
            = DependencyProperty.RegisterAttached("TypeFormat", typeof(TextBoxTypeEnum), typeof(TextBoxHelpers),
                new PropertyMetadata(TextBoxTypeEnum.Default, TypeFormatChanged));

        public static TextBoxTypeEnum GetTypeFormat(DependencyObject dependencyObject)
        {
            return (TextBoxTypeEnum) dependencyObject.GetValue(TypeFormatProperty);
        }

        public static void SetTypeFormat(DependencyObject dependencyObject, TextBoxTypeEnum value)
        {
            dependencyObject.SetValue(TypeFormatProperty, value);
        }

        public static void TypeFormatChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var textBox = dependencyObject as TextBox;
            if (null == textBox) return;
            var newValue = (TextBoxTypeEnum) dependencyPropertyChangedEventArgs.NewValue;
            switch (newValue)
            {
                case TextBoxTypeEnum.Decimal:
                {
                    dependencyObject.SetValue(IsNumericProperty, false);
                    dependencyObject.SetValue(IsSerialProperty, false);
                    dependencyObject.SetValue(IsDecimalProperty, true);
                    break;
                }
                case TextBoxTypeEnum.Numeric:
                {
                    dependencyObject.SetValue(IsDecimalProperty, false);
                    dependencyObject.SetValue(IsSerialProperty, false);
                    dependencyObject.SetValue(IsNumericProperty, true);
                    break;
                }
                case TextBoxTypeEnum.Serial:
                {
                    dependencyObject.SetValue(IsDecimalProperty, false);
                    dependencyObject.SetValue(IsNumericProperty, false);
                    dependencyObject.SetValue(IsSerialProperty, true);
                    break;
                }
                case TextBoxTypeEnum.Default:
                {
                    dependencyObject.SetValue(IsDecimalProperty, false);
                    dependencyObject.SetValue(IsNumericProperty, false);
                    dependencyObject.SetValue(IsSerialProperty, false);
                    break;
                }
            }
        }

        #endregion TypeFormatProperty


        #region IgnoreCharProperty

        public static string GetIgnoreChar(DependencyObject obj)
        {
            return (string) obj.GetValue(IgnoreCharProperty);
        }

        public static void SetIgnoreChar(DependencyObject obj, string value)
        {
            obj.SetValue(IgnoreCharProperty, value);
        }

        /// <summary>
        ///     Массив запрещённых символов
        /// </summary>
        public static readonly DependencyProperty IgnoreCharProperty =
            DependencyProperty.RegisterAttached("IgnoreChar", typeof(string), typeof(TextBoxHelpers),
                new PropertyMetadata(string.Empty, (s, e) =>
                {
                    var targetTextbox = s as TextBox;
                    if (targetTextbox != null)
                    {
                        if (!((string) e.OldValue).IsNullOrEmpty() && ((string) e.NewValue).IsNullOrEmpty())
                            targetTextbox.PreviewTextInput -= targetTextbox_PreviewTextInputIgnoreChar;
                        if (!((string) e.NewValue).IsNullOrEmpty())
                        {
                            targetTextbox.PreviewTextInput += targetTextbox_PreviewTextInputIgnoreChar;
                            if (((string) e.NewValue).Contains(' '))
                                targetTextbox.PreviewKeyDown += targetTextbox_PreviewKeyDown;
                        }
                    }
                }));

        private static void targetTextbox_PreviewTextInputIgnoreChar(object sender, TextCompositionEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Text))
            {
                var ignore = GetIgnoreChar((TextBox) sender);
                var newChar = e.Text[0];
                e.Handled = ignore.Contains(newChar);
            }
            else
            {
                e.Handled = true;
            }
        }

        #endregion IgnoreCharProperty
    }
}