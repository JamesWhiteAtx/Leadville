/*
Password Strength Indicator using jQuery and XML

By: Bryian Tan (bryian.tan at ysatech.com)
01122011 - v01.01.00
01172011 - v01.02.00
07232012 - v01.03.00

Description:
Password Strength Indicator somewhat similar to ASP.NET AJAX PasswordStrength extender control behavior 

Resources:
http://www.codeproject.com/Articles/430777/ASP-NET-MVC3-Password-Strength-Indicator-using-jQu
http://fyneworks.blogspot.com/2007/04/dynamic-regular-expressions-in.html
http://projects.sharkmediallc.com/pass/
http://docs.jquery.com/Plugins/Authoring
http://stackoverflow.com/questions/1034306/public-functions-from-within-a-jquery-plugin
*/

(function ($) {

    var password_Strength = new function () {

        //return count that match the regular expression
        this.countRegExp = function (passwordVal, regx) {
            var match = passwordVal.match(regx);
            return match ? match.length : 0;
        }

        this.getStrengthInfo = function (passwordVal) {
            var len = passwordVal.length;
            var pStrength = 0; //password strength
            var msg = "", inValidChars = ""; //message

            //get special characters from xml file
            var allowableSpecilaChars = new RegExp("[" + password_settings.specialChars + "]", "g")

            var nums = this.countRegExp(passwordVal, /\d/g), //numbers
			lowers = this.countRegExp(passwordVal, /[a-z]/g),
			uppers = this.countRegExp(passwordVal, /[A-Z]/g), //upper case
			specials = this.countRegExp(passwordVal, allowableSpecilaChars), //special characters
			spaces = this.countRegExp(passwordVal, /\s/g);

            //check for invalid characters
            inValidChars = passwordVal.replace(/[a-z]/gi, "") + inValidChars.replace(/\d/g, "");
            inValidChars = inValidChars.replace(/\d/g, "");
            inValidChars = inValidChars.replace(allowableSpecilaChars, "");

            //check space
            if (spaces > 0) {
                return "No spaces!";
            }

            //invalid characters
            if (inValidChars !== '') {
                return "Invalid character: " + inValidChars;
            }

            //max length
            if (len > password_settings.maxLength) {
                return "Password too long!";
            }

            //GET NUMBER OF CHARACTERS left
            if ((specials + uppers + nums + lowers) < password_settings.minLength) {
                msg += password_settings.minLength - (specials + uppers + nums + lowers) + " more characters, ";
            }

            //at the "at least" at the front
            if (specials == 0 || uppers == 0 || nums == 0 || lowers == 0) {
                msg += "At least ";
            }

            //GET NUMBERS
            if (nums >= password_settings.numberLength) {
                nums = password_settings.numberLength;
            }
            else {
                msg += (password_settings.numberLength - nums) + " more numbers, ";
            }

            //special characters
            if (specials >= password_settings.specialLength) {
                specials = password_settings.specialLength
            }
            else {
                msg += (password_settings.specialLength - specials) + " more special character, ";
            }

            //upper case letter
            if (uppers >= password_settings.upperLength) {
                uppers = password_settings.upperLength
            }
            else {
                msg += (password_settings.upperLength - uppers) + " upper case characters, ";
            }

            //strength for length
            if ((len - (uppers + specials + nums)) >= (password_settings.minLength - password_settings.numberLength - password_settings.specialLength - password_settings.upperLength)) {
                pStrength += (password_settings.minLength - password_settings.numberLength - password_settings.specialLength - password_settings.upperLength);
            }
            else {
                pStrength += (len - (uppers + specials + nums));
            }

            //password strength
            pStrength += uppers + specials + nums;

            //detect missing lower case character
            if (lowers === 0) {
                if (pStrength > 1) {
                    pStrength -= 1; //Reduce 1
                }
                msg += "1 lower case character, ";
            }

            //strong password
            if (pStrength == password_settings.minLength && lowers > 0) {
                msg = "Strong password!";
            }

            return msg + ';' + pStrength;
        }
    }

    //default setting
    var password_settings = {
        minLength: 12,
        maxLength: 25,
        specialLength: 1,
        upperLength: 1,
        numberLength: 1,
        barWidth: 200,
        barColor: 'Red',
        specialChars: '!@#$', //allowable special characters
        metRequirement: false,
        useMultipleColors: 0
    };
    var selectr = null;
    var textContainer = null;
    var strengthBorder = null;
    var strengthBar = null;

    //password strength plugin 
    $.fn.password_strength = function (settingsUrl, borderDiv, barDiv, msgSpan) {
        if (borderDiv) {
            borderDiv.hide("fast");
            borderDiv.addClass("password-strength-border");
        };
        if (barDiv) {
            barDiv.hide("fast");
            barDiv.addClass("password-strength-bar");
        };
        if (msgSpan) {
            msgSpan.hide("fast");
        };

        //check if password met requirement
        this.metReq = function () {
            return password_settings.metRequirement;
        }

        selectr = $(this);
        $.getJSON(settingsUrl, function (result) {
            if (result) {
                password_settings.minLength = result.MinLength;
                password_settings.maxLength = result.MaxLength;
                password_settings.specialLength = result.SpecialLength;
                password_settings.upperLength = result.UpperLength;
                password_settings.numberLength = result.NumsLength;
                password_settings.barWidth = result.BarWidth;
                password_settings.barColor = result.BarColor;
                password_settings.specialChars = result.SpecialChars;
                password_settings.useMultipleColors = result.UseMultipleColors;
                if (selectr) {
                    selectr.attr('maxLength', password_settings.maxLength);
                };
            };
        });

        return this.each(function () {

            if (!((borderDiv) && (barDiv) && (msgSpan))) {

                //bar position
                var barLeftPos = $("[id='" + this.id + "']").position().left + $("[id='" + this.id + "']").width();
                var barTopPos = $("[id='" + this.id + "']").position().top + $("[id='" + this.id + "']").height();

                //password indicator text message span
                textContainer = $('<span></span>')
                    .css({ position: 'absolute', top: barTopPos - 6, left: barLeftPos + 15, 'font-size': '75%', display: 'inline-block', width: password_settings.barWidth + 40 });

                //add the message span next to textbox
                $(this).after(textContainer);

                //bar border and indicator div
                var barDivs = $('<div id="PasswordStrengthBorder"></div><div id="PasswordStrengthBar" class="BarIndicator"></div>')
                    .css({ position: 'absolute', display: 'none' })
                    .eq(0).css({ height: 3, top: barTopPos - 16, left: barLeftPos + 15, 'border-style': 'solid', 'border-width': 1, padding: 2 }).end()
                    .eq(1).css({ height: 5, top: barTopPos - 14, left: barLeftPos + 17 }).end()

                //add the boder and div
                textContainer.before(barDivs);

                strengthBorder = $("[id='PasswordStrengthBorder']");
                strengthBar = $("[id='PasswordStrengthBar']");
            };

            //set max length of textbox
            $(this).attr('maxLength', password_settings.maxLength); // use default until json call returns with app settings

            $(this).keyup(function () { 
                calculateStrength($(this)); 
            });

            function calculateStrength(keyUpSelectr) {

                var passwordVal = keyUpSelectr.val(); //get textbox value

                //set met requirement to false
                password_settings.metRequirement = false;

                if (passwordVal.length > 0) {

                    var msgNstrength = password_Strength.getStrengthInfo(passwordVal);

                    var msgNstrength_array = msgNstrength.split(";"), strengthPercent = 0,
                    barWidth = password_settings.barWidth,
                    backColor = password_settings.barColor;

                    //calculate the bar indicator length
                    if (msgNstrength_array.length > 1) {
                        strengthPercent = (msgNstrength_array[1] / password_settings.minLength) * barWidth;
                    }

                    //use multiple colors
                    if (password_settings.useMultipleColors == 1) {
                        //first 33% is red
                        if (parseInt(strengthPercent) >= 0 && parseInt(strengthPercent) <= (barWidth * .33)) {
                            backColor = "red";
                        }
                        //33% to 66% is blue
                        else if (parseInt(strengthPercent) >= (barWidth * .33) && parseInt(strengthPercent) <= (barWidth * .67)) {
                            backColor = "blue";
                        }
                        else {
                            backColor = password_settings.barColor;
                        }
                    }

                    //remove last "," character
                    var msgTxt;
                    if (msgNstrength_array[0].lastIndexOf(",") !== -1) {
                        msgTxt = msgNstrength_array[0].substring(0, msgNstrength_array[0].length - 2);
                    }
                    else {
                        msgTxt = msgNstrength_array[0];
                    }

                    if (typeof clearErrorValidation == 'function') {
                        clearErrorValidation(keyUpSelectr.attr('id'));
                    }

                    if (strengthBorder) {
                        //$("[id='PasswordStrengthBorder']").css({ display: 'inline', width: barWidth });
                        strengthBorder.css({ display: 'inline', width: barWidth });
                    };
                    if (borderDiv) {
                        borderDiv.show();
                        borderDiv.css({ width: barWidth });
                    };
                    if (strengthBar) {
                        //$("[id='PasswordStrengthBar']").css({ display: 'inline', width: strengthPercent, 'background-color': backColor });
                        strengthBar.css({ display: 'inline', width: strengthPercent, 'background-color': backColor });
                    };
                    if (barDiv) {
                        barDiv.show();
                        barDiv.css({ width: strengthPercent, 'background-color': backColor });
                    };
                    if (textContainer) {
                        textContainer.text(msgTxt);
                    };
                    if (msgSpan) {
                        msgSpan.show();
                        msgSpan.text(msgTxt);
                    };

                    if (strengthPercent == barWidth) {
                        password_settings.metRequirement = true;
                    }

                }
                else {
                    if (textContainer) {
                        textContainer.text('');
                    };
                    if (msgSpan) {
                        msgSpan.text('');
                    };
                    if (strengthBorder) {
                        //$("[id='PasswordStrengthBorder']").css("display", "none"); //hide
                        strengthBorder.css("display", "none"); //hide
                    };
                    if (borderDiv) {
                        borderDiv.hide("fast");
                    };
                    if (strengthBar) {
                        //$("[id='PasswordStrengthBar']").css("display", "none"); //hide
                        strengthBar.css("display", "none"); //hide
                    };
                    if (barDiv) {
                        barDiv.hide("fast");
                    };
                }
            };
        });
    };

})(jQuery);
