Contrib.CookieCuttr
===================

The CookieCuttr plugin for Orchard (http://cookiecuttr.com/)

Dependencies
============

Orchard.jQuery

Quick Setup
===========

Enable the CookieCuttr Module.<br />
Go to Widgets and select Default Layer.<br />
Press the 'Add' button to any zone you like. (Footer is ok)<br />
Select Cookiecuttr Widget from the List of widgets.<br />
Enter a title for the widget. e.g. Cookies, and uncheck the tickbox to hide the title.<br />
Ready, you are ok with the EU cookie Law.<br />

Available Options
=================

For full options of the javascript plugin, visit http://cookiecuttr.com#options

<strong>cookieAnalytics</strong> - if you are just using a simple analytics package you can set this to true, it displays a simple default message with no privacy policy link - this is set to true by default.

<strong>cookieDeclineButton</strong> - if you’d like a decline button to (ironically) write a cookie into the browser then set this to true.

<strong>cookieAcceptButton</strong> - set this to true to hide the accept button, its set to false by default

<strong>cookieResetButton</strong> - if you’d like a reset button to delete the accept or decline cookies then set this to true.

<strong>cookiePolicyLink</strong> - if applicable, enter the link to your privacy policy in here - this is as soon as cookieAnalytics is set to false;

<strong>cookieMessage</strong> - edit the message you want to appear in the cookie bar, remember to keep the {{cookiePolicyLink}} variable in tact so it inserts your privacy policy link.

<strong>cookieAnalyticsMessage</strong> - edit the message you want to appear, this is the default message.

<strong>cookieWhatAreTheyLink</strong> - edit the link for the 'What are Cookies' link.

<strong>cookieWhatAreLinkText</strong> - you can change the text of the "What are Cookies" link shown on Google Analytics message.

<strong>cookieNotificationLocationBottom</strong> - this is false by default, change it to true and the cookie notification bar will show at the bottom of the page instead, please note this will remain at the top for mobile and iOS devices and Internet Explorer 6.

<strong>cookieAcceptButtonText</strong> - you can change the text of the green accept button.

<strong>cookieDeclineButtonText</strong> - you can change the text of the red decline button.

<strong>cookieResetButtonText</strong> - you can change the text of the orange reset button.

<strong>cookiePolicyPage</strong> - set this to true to display the message you want to appear on your privacy or cookie policy page.

<strong>cookiePolicyPageMessage</strong> - edit the message you want to appear on your policy page.

<strong>cookieDiscreetLink</strong> - false by default, set to true to enable.

<strong>cookieDiscreetLinkText</strong> - edit the text you want to appear on the discreet option.

<strong>cookieDiscreetPosition</strong> - set to topleft by default, you can also set topright, bottomleft, bottomright.
