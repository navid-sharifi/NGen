﻿namespace NGen
{
    public class FileManager : AModule
    {
        public override string GetReactPage(Type pageType, Type moduleType)
        {
            return @$"
import React from 'react';
import React from ""react"";
import Button from ""react-bootstrap/Button"";
import Modal from ""react-bootstrap/Modal"";
import {{ NPostData }} from ""../../../Tools/Extentions"";
import ""./FileManagerModule.scss"";

const {GetReactModuleName(pageType, moduleType)} = () => {{

    return (
<div>
<h1>im react FileManager</h1>
</div>);
}}

export default {GetReactModuleName(pageType, moduleType)};";

        }
    }
}
