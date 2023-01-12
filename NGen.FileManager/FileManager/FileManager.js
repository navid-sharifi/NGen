import React from "react";
import Button from "react-bootstrap/Button";
import Modal from "react-bootstrap/Modal";
import { NPostData } from "../../Tools/Extentions";
import "./FileManagerModule.scss";

const FileManagerModule = ({ onChange, value, ClassName, placeholder })=> {


    const [modalShow, setModalShow] = React.useState(false);
     return (<>
         <div class="input-group" onClick={() => setModalShow(true)} >
             <div class="input-group-prepend">
                 <div class="input-group-text">
                     <svg xmlns="http://www.w3.org/2000/svg" width="25" viewBox="0 0 50 50">
                         <path d="M0 4L0 46L50 46L50 4 Z M 5 9L45 9L45 41L5 41 Z M 33 15C30.792969 15 29 16.792969 29 19C29 21.207031 30.792969 23 33 23C35.207031 23 37 21.207031 37 19C37 16.792969 35.207031 15 33 15 Z M 17.03125 16C16.726563 15.992188 16.449219 16.109375 16.25 16.34375L7 27.15625L7 39L43 39L43 29.125L35.375 26.0625C35.113281 25.957031 34.820313 25.96875 34.5625 26.09375L27.3125 29.71875L17.8125 16.40625C17.632813 16.15625 17.339844 16.015625 17.03125 16Z" fill="#5B5B5B" />
                     </svg>
                 </div>
             </div>
             <input
                 onChange={onChange}
                 value={value}
                 className={ClassName}
                 placeholder={placeholder} />
         </div>
         
        <div className="filemanager-module">
            <button className="btn p-0" onClick={() => setModalShow((c) => !c)}
                style={{
                    right: "10px",
                    position: "fixed",
                    bottom: "10px",
                    borderRadius: "100%",
                    width: "90px",
                    height: "90px",
                }}>
                <svg fill="#ffae00" viewBox="-1 0 19 19" xmlns="http://www.w3.org/2000/svg" class="cf-icon-svg" stroke="#ffae00"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_iconCarrier"><path d="M16.417 9.579A7.917 7.917 0 1 1 8.5 1.662a7.917 7.917 0 0 1 7.917 7.917zm-3.439-3.072H7.752l-.436-.852a.57.57 0 0 0-.461-.282H4.038a.318.318 0 0 0-.317.317v7.438a.318.318 0 0 0 .317.316h8.94a.317.317 0 0 0 .317-.316V6.824a.317.317 0 0 0-.317-.317z"></path></g></svg>
            </button>

            <MyVerticallyCenteredModal show={modalShow} onHide={() => setModalShow(false)} />
         </div>
         </>
    );
};

export default FileManagerModule;


function MyVerticallyCenteredModal(props) {
    const [AddFolderForm, SetAddFolderForm] = React.useState(false);
    const [AddFileForm, SetAddFileForm] = React.useState(false);
    const [FileSource, SetFile] = React.useState(null);
    const [FolderName, SetFolderName] = React.useState("");
    const [FileName, SetFileName] = React.useState("");
    const [Row, setRow] = React.useState([]);
    const [CurrentFolder, SetCurrentFolder] = React.useState("");
    const [UpIds, setUpId] = React.useState([]);

    React.useEffect(() => {
        UpdateRows("", CurrentFolder);
    }, [CurrentFolder]);

    const UpdateRows = (search = "", folderid = "") => {
        var data = {};
        data["Find"] = search;
        if (folderid) {
            data["FolderId"] = folderid;
        }
        NPostData("/FileManager/GetFilesAndFolders", data).then((rows) => {
            setRow(rows);
        });
    };

    const OnCreatFolder = (event) => {
        var data = {};
        data["name"] = FolderName;
        if (CurrentFolder) {
            data["FatherId"] = CurrentFolder;
        }
        NPostData("/FileManager/AddFolder", data).then((r) => {
            console.log(r);
            SetFolderName("");
            SetAddFolderForm(false);
            UpdateRows("", CurrentFolder);
        });
    };

    const OnAddFile = (event) => {
        var data = new FormData();
        data.append("File", FileSource);

        if (CurrentFolder) {
            data.append("folder", CurrentFolder);
        }
        if (FileName) {
            data.append("Name", FileName);
        }

        NPostData("/FileManager/AddFile", data, {
            "Content-Type": "multipart/form-data",
        }).then((r) => {
            SetFileName("");
            SetFile(null);
            SetAddFileForm(false);
            UpdateRows("", CurrentFolder);
        });
    };

    return (
        <Modal
            {...props}
            dialogClassName="modal-90w"
            aria-labelledby="example-custom-modal-styling-title"
        >
            <Modal.Header closeButton>
                <Modal.Title id="example-custom-modal-styling-title">
                    File Manager
                </Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <div className="fileManager-group-button">
                    <Button
                        variant="primary"
                        className="m-2"
                        onClick={() => {
                            if (AddFileForm) {
                                SetAddFileForm((c) => !c);
                            }
                            SetAddFolderForm((c) => !c);
                        }}
                    >
                        + New Folder
                    </Button>
                    <Button
                        onClick={() => {
                            if (AddFolderForm) {
                                SetAddFolderForm((c) => !c);
                            }
                            SetAddFileForm((c) => !c);
                        }}
                        variant="primary"
                        className="m-2"
                    >
                        + New File
                    </Button>
                </div>
                {/* /////////////////// */}
                {AddFileForm ? (
                    <form>
                        <div className="alert alert-primary" style={{ margin: " 5px" }}>
                            <h5>Add file</h5>
                            <div
                                style={{
                                    display: "grid",
                                    grid: "100% / 50px auto",
                                    padding: "10px 10px 20px 0",
                                }}
                            >
                                <div>Name</div>
                                <div style={{ padding: "0 20px 0 20px" }}>
                                    <input
                                        style={{ width: "100%" }}
                                        onChange={(event) => SetFileName(event.target.value)}
                                    />
                                </div>
                            </div>
                            <div
                                style={{
                                    display: "grid",
                                    grid: "100% /50px auto",
                                    padding: "10px 10px 20px 0",
                                }}
                            >
                                <div>File</div>
                                <div style={{ padding: "0 20px 0 20px" }}>
                                    <input
                                        type="file"
                                        style={{ width: "100%" }}
                                        onChange={(e) => SetFile(e.target.files[0])}
                                    />
                                </div>
                            </div>
                            <div style={{ textAlign: "right" }}>
                                <Button
                                    style={{ zoom: 0.9, marginTop: 0, paddingTop: 0 }}
                                    onClick={OnAddFile}
                                >
                                    Save
                                </Button>
                            </div>
                        </div>
                    </form>
                ) : null}
                {/* //////////////// */} {/* /////////////////// */}
                {AddFolderForm ? (
                    <form>
                        <div className="alert alert-primary" style={{ margin: " 5px" }}>
                            <h5>Add Folder</h5>
                            <div
                                style={{
                                    display: "grid",
                                    grid: "100% / 50px auto",
                                    padding: "10px 10px 20px 0",
                                }}
                            >
                                <div>Name</div>
                                <div style={{ padding: "0 20px 0 20px" }}>
                                    <input
                                        style={{ width: "100%" }}
                                        onChange={(e) => SetFolderName(e.target.value)}
                                    />
                                </div>
                            </div>
                            <div style={{ textAlign: "right" }}>
                                <Button
                                    style={{ zoom: 0.9, marginTop: 0, paddingTop: 0 }}
                                    onClick={OnCreatFolder}
                                >
                                    Save
                                </Button>
                            </div>
                        </div>
                    </form>
                ) : null}
                {/* //////////// //// */}
                {UpIds && UpIds.length > 0 ? (
                    <RowUpFolder
                        fatherId={UpIds}
                        SetCurrentFolder={SetCurrentFolder}
                        UpIds={UpIds}
                        setUpId={setUpId}
                    />
                ) : null}
                {Row && Row.length > 0
                    ? Row.map((k, i) => {
                        switch (k.type) {
                            case "file":
                                return <RowFile data={k} />;
                            case "folder":
                                return (
                                    <RowFolder
                                        data={k}
                                        SetCurrentFolder={SetCurrentFolder}
                                        setUpId={setUpId}
                                    />
                                );
                            default:
                                return null;
                        }
                    })
                    : null}
                <div></div>
            </Modal.Body>
        </Modal>
    );
}

const rowStyle = { display: "grid", grid: "100% 10px / 35px auto" };

const RowFile = ({ data }) => {
    const [Option, SetOption] = React.useState(false);
    return (
        <div
            className={Option ? "fileManager-row-active" : "fileManager-row"}
            onClick={() => SetOption((c) => !c)}
        >
            <div style={rowStyle}>
                <File />
                <div>{data.name}</div>
            </div>
            {Option ? (
                <div>
                    <div style={{ zoom: 0.8, textAlign: "right" }}>
                        <Button className="m-1">rename</Button>
                        <Button className="m-1">delete</Button>
                        <Button
                            className="m-1"
                            href={`/FileManager/download/${data.random}/${data.id}`}
                        >
                            download
                        </Button>
                        <Button className="m-1">Copy Adress</Button>
                    </div>
                </div>
            ) : null}
        </div>
    );
};

const RowFolder = ({ data, SetCurrentFolder, setUpId }) => {
    const [Option, SetOption] = React.useState(false);
    console.log(data);
    return (
        <div
            className={Option ? "fileManager-row-active" : "fileManager-row"}
            onClick={() => SetOption((c) => !c)}
        >
            <div style={rowStyle}>
                <Folder />
                <div>{data.name}</div>
            </div>
            {Option ? (
                <div>
                    <div style={{ zoom: 0.8, textAlign: "right" }}>
                        <Button
                            className="m-1"
                            onClick={() => {
                                SetCurrentFolder(data.id);
                                setUpId((c) => [...c, data.fatherId ?? ""]);
                            }}
                        >
                            Open
                        </Button>
                        <Button className="m-1">delete</Button>
                        <Button className="m-1">download</Button>
                        <Button className="m-1">Copy Adress</Button>
                    </div>
                </div>
            ) : null}
        </div>
    );
};

const RowUpFolder = ({ fatherId, SetCurrentFolder, setUpId, UpIds = [] }) => {
    console.log(fatherId);
    return (
        <div
            className="fileManager-row"
            onClick={() => {
                var newUpIds = UpIds;
                var removed = newUpIds.splice(-1);
                SetCurrentFolder(removed[0]);

                setUpId(newUpIds);
            }}
        >
            <div style={rowStyle}>
                <Folder />
                <div>...</div>
            </div>
        </div>
    );
};

const Folder = () => {
    return (
        <div>
            <svg
                width={30}
                viewBox="0 -1 22 22"
                id="meteor-icon-kit__solid-folder"
                fill="none"
                xmlns="http://www.w3.org/2000/svg"
            >
                <g id="SVGRepo_bgCarrier" stroke-width="0"></g>
                <g id="SVGRepo_iconCarrier">
                    <path
                        fill-rule="evenodd"
                        clip-rule="evenodd"
                        d="M8.39445 0C8.7288 0 9.041 0.1671 9.2265 0.4453L10.6328 2.5547C10.8182 2.8329 11.1305 3 11.4648 3H20C21.1046 3 22 3.89543 22 5V18C22 19.1046 21.1046 20 20 20H2C0.89543 20 0 19.1046 0 18L0 2C0 0.89543 0.89543 0 2 0L8.39445 0z"
                        fill="#f1c00e"
                    ></path>
                </g>
            </svg>
        </div>
    );
};

const File = () => {
    return (
        <div>
            <svg
                width={30}
                viewBox="-3 0 24 24"
                id="meteor-icon-kit__solid-file"
                fill="none"
                xmlns="http://www.w3.org/2000/svg"
            >
                <g id="SVGRepo_bgCarrier" stroke-width="0"></g>
                <g id="SVGRepo_iconCarrier">
                    <path
                        fill-rule="evenodd"
                        clip-rule="evenodd"
                        d="M0 6H5C5.55228 6 6 5.55228 6 5V0H16C17.1046 0 18 0.89543 18 2V22C18 23.1046 17.1046 24 16 24H2C0.89543 24 0 23.1046 0 22V6zM0.34141 4C0.94398 2.29517 2.29517 0.943981 4 0.341411V4H0.34141z"
                        fill="#758CA3"
                    ></path>
                </g>
            </svg>
        </div>
    );
};
