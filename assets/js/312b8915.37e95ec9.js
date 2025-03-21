"use strict";(self.webpackChunk=self.webpackChunk||[]).push([[384],{3331:(e,t,n)=>{n.d(t,{R:()=>r,x:()=>a});var o=n(8101);const s={},i=o.createContext(s);function r(e){const t=o.useContext(i);return o.useMemo((function(){return"function"==typeof e?e(t):{...t,...e}}),[t,e])}function a(e){let t;return t=e.disableParentContext?"function"==typeof e.components?e.components(s):e.components||s:r(e.components),o.createElement(i.Provider,{value:t},e.children)}},8667:(e,t,n)=>{n.r(t),n.d(t,{assets:()=>d,contentTitle:()=>a,default:()=>c,frontMatter:()=>r,metadata:()=>o,toc:()=>h});const o=JSON.parse('{"id":"troubleshoot","title":"Troubleshooting","description":"This section is for common issues you may encounter and how to resolve them.","source":"@site/docs/troubleshoot.md","sourceDirName":".","slug":"/troubleshoot","permalink":"/TF2HUD.Editor/troubleshoot","draft":false,"unlisted":false,"editUrl":"https://github.com/CriticalFlaw/TF2HUD.Editor/tree/master/docs/troubleshoot.md","tags":[],"version":"current","lastUpdatedAt":1742562077000,"frontMatter":{"title":"Troubleshooting"}}');var s=n(5105),i=n(3331);const r={title:"Troubleshooting"},a=void 0,d={},h=[{value:"The latest release does not contain the executable.",id:"the-latest-release-does-not-contain-the-executable",level:3},{value:"The editor does not launch after downloading and extracting it.",id:"the-editor-does-not-launch-after-downloading-and-extracting-it",level:3},{value:"Access to the path ... .dll is denied.",id:"access-to-the-path--dll-is-denied",level:3},{value:"Customization changes are not being shown in-game.",id:"customization-changes-are-not-being-shown-in-game",level:3},{value:"Error when applying or resetting HUD customizations.",id:"error-when-applying-or-resetting-hud-customizations",level:3},{value:"Access to the path ... temp.zip is denied.",id:"access-to-the-path--tempzip-is-denied",level:3},{value:"Could not find a part of the path &quot;..tf/custom&quot;.",id:"could-not-find-a-part-of-the-path-tfcustom",level:3}];function l(e){const t={a:"a",admonition:"admonition",code:"code",h3:"h3",hr:"hr",li:"li",p:"p",strong:"strong",ul:"ul",...(0,i.R)(),...e.components};return(0,s.jsxs)(s.Fragment,{children:[(0,s.jsx)(t.p,{children:"This section is for common issues you may encounter and how to resolve them."}),"\n",(0,s.jsxs)(t.ul,{children:["\n",(0,s.jsxs)(t.li,{children:["For issues not on this page, please ",(0,s.jsx)(t.a,{href:"https://github.com/CriticalFlaw/TF2HUD.Editor/issues",children:"open a ticket on our issue tracker"}),"."]}),"\n",(0,s.jsxs)(t.li,{children:["For questions not covered in the documentation, ",(0,s.jsx)(t.a,{href:"https://discord.gg/hTdtK9vBhE",children:"visit our Discord server"}),"."]}),"\n"]}),"\n",(0,s.jsx)(t.hr,{}),"\n",(0,s.jsx)(t.h3,{id:"the-latest-release-does-not-contain-the-executable",children:"The latest release does not contain the executable."}),"\n",(0,s.jsxs)(t.p,{children:["You most likely downloaded the source code instead of the editor. On the ",(0,s.jsx)(t.a,{href:"https://github.com/CriticalFlaw/TF2HUD.Editor/releases",children:"releases"})," page, make sure to download the file named ",(0,s.jsx)(t.strong,{children:"tf2-hud-editor_X.X.zip"})," and extract it into a separate folder."]}),"\n",(0,s.jsx)(t.hr,{}),"\n",(0,s.jsx)(t.h3,{id:"the-editor-does-not-launch-after-downloading-and-extracting-it",children:"The editor does not launch after downloading and extracting it."}),"\n",(0,s.jsxs)(t.p,{children:["Make sure to install the version of ",(0,s.jsx)(t.a,{href:"https://dotnet.microsoft.com/download/dotnet/8.0/runtime",children:"Microsoft .NET 8.0 Runtime"})," intended for running ",(0,s.jsx)(t.strong,{children:"desktop apps"}),". If you just installed it and the editor still does not launch, then restart your system."]}),"\n",(0,s.jsx)(t.hr,{}),"\n",(0,s.jsx)(t.h3,{id:"access-to-the-path--dll-is-denied",children:"Access to the path ... .dll is denied."}),"\n",(0,s.jsx)(t.p,{children:"Make sure TF2HUD.Editor is located on your main drive instead of an external drive."}),"\n",(0,s.jsx)(t.admonition,{type:"note",children:(0,s.jsx)(t.p,{children:"If your TF2 installation is located on an external drive you may need to set your tf/custom directory in the editor."})}),"\n",(0,s.jsx)(t.hr,{}),"\n",(0,s.jsx)(t.h3,{id:"customization-changes-are-not-being-shown-in-game",children:"Customization changes are not being shown in-game."}),"\n",(0,s.jsxs)(t.p,{children:["To see your selected changes in-game, open the console and input ",(0,s.jsx)(t.code,{children:"hud_reloadscheme"}),". This will refresh the HUD with your selected customizations."]}),"\n",(0,s.jsx)(t.admonition,{type:"note",children:(0,s.jsx)(t.p,{children:"Certain settings may require the game to be restarted, this mainly applies to color and main menu changes. If the game is running, a message will display notifying you that a game restart is required."})}),"\n",(0,s.jsx)(t.hr,{}),"\n",(0,s.jsx)(t.h3,{id:"error-when-applying-or-resetting-hud-customizations",children:"Error when applying or resetting HUD customizations."}),"\n",(0,s.jsx)(t.p,{children:"Most errors you'll encounter will be caused by an outdated version of the HUD being installed. An outdated HUD may not have the latest changes that the editor would expect and when that happens, an error is returned. Best thing to do is reinstall the HUD through the editor and reapply the customizations."}),"\n",(0,s.jsx)(t.hr,{}),"\n",(0,s.jsx)(t.h3,{id:"access-to-the-path--tempzip-is-denied",children:"Access to the path ... temp.zip is denied."}),"\n",(0,s.jsx)(t.p,{children:"Try running the editor as administrator."}),"\n",(0,s.jsxs)(t.p,{children:["If that didn't work, double-check that your antivirus program isn't denying access to the editor program. ",(0,s.jsx)(t.a,{href:"https://github.com/CriticalFlaw/TF2HUD.Editor/issues/107",children:"For example,"})," Avast has been known to prevent the editor from creating files in certain paths."]}),"\n",(0,s.jsx)(t.hr,{}),"\n",(0,s.jsx)(t.h3,{id:"could-not-find-a-part-of-the-path-tfcustom",children:'Could not find a part of the path "..tf/custom".'}),"\n",(0,s.jsx)(t.p,{children:"This can happen when TF2 is installed on a separate drive from your main Steam installation."}),"\n",(0,s.jsxs)(t.p,{children:["If the app does not find the directory to ",(0,s.jsx)(t.code,{children:"tf/custom"}),', it should prompt you to set the path manually. If that does not happen, users will have to click on the wrench icon at near the bottom of screen to open the Options menu then select "Set path to tf/custom". You\'ll then need to navigate to your TF2 installation folder, select tf/custom and click Select Folder.']}),"\n",(0,s.jsxs)(t.p,{children:["For HUD Editor versions 2.5 and lower, please refer to this video: ",(0,s.jsx)(t.a,{href:"https://www.youtube.com/watch?v=NqSqLyROBwk",children:"https://www.youtube.com/watch?v=NqSqLyROBwk"})]})]})}function c(e={}){const{wrapper:t}={...(0,i.R)(),...e.components};return t?(0,s.jsx)(t,{...e,children:(0,s.jsx)(l,{...e})}):l(e)}}}]);