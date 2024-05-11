Our CSS file is split in to the following sections

Theme: Sets colours and fonts for easy changing of theme
Tag Styles: sets the style of all base html elements e.g h1, input, button, table etc.
Utils: defines helper classes for layouts and common used styles e.g. margin sizes or error message styles
Components: These are the styles of each component from a design doc or Figma. This is where we would set up multplie different button or input
            styles or add styles for cards, avatars etc.

If an app gets complicated we can split these in to multiple files if needed.

There can be extra css in vue components or pages that are specific to that particular component. 
E.g. a modal component might have it's css in the vue file so it's easy to reuse in multiple projects.

We would prefer to use CSS nesting but a lot of browser versions still don't support it.

We don't use SASS, Tailwind or any other css framework that requires a compile step

px to rem guide for common sizes 
0.25rem; 4px; 
0.5rem;  8px; 
0.625rem; 10px; 
0.75rem; 12px;
0.8125rem; 13px
0.875rem; 14px
0.938rem; 15px; 
1rem; 16px;
1.125rem;  18px; 
1.25rem;  20px; 
1.375rem; 22px; 
1.5rem; 24px; 
1.563rem;  25px;
1.688rem; 27px; 
1.875rem; 30px;
2.375rem; 38px
2.5rem; 40px; 
3rem; 48px; 
3.313rem;  53px;  
3.375rem; 54px; 
3.5rem; 56px; 
3.75rem; 60px; 
4rem; 64px
4.5rem; 72px;
5rem;  80px; 
6.25rem; 100px;
6.5rem; 104px
7.5rem;  120px; 
16.25rem;   260px; 
52.5rem; 840px

/* breakpoint styles */

/* < 568px mobile */

/* >= 568px not seen used */
/* @media screen and (min-width: 35.5em) {
} */

/* >= 768px tablet (ipad portrait) / half window width + most phones in landscape */
/* @media screen and (min-width: 48em) 	{
} */

/* >= 1024px tablet (ipad landscape/ just over half window width */
/* @media screen and (min-width: 64em) {
} */

/* >= 1280px / 150% windows zoom 
@media screen and (min-width: 80em) {
  
}
*/

/* >= 1536px 125% windows zoom 
@media screen and (min-width: 96em) {

}
*/

/* >= 1920px / no windows zoom 
@media screen and (min-width: 120em) {
  
}

*/