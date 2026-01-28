window.quillEditors = {};

window.initQuill = (editorId) => {
    const quill = new Quill(`#${editorId}`, {
        theme: "snow",
        modules: {
            toolbar: [
                ["bold", "italic", "underline"],
                [{ list: "ordered" }, { list: "bullet" }],
                [{ header: [1, 2, false] }],
                ["clean"]
            ]
        }
    });

    window.quillEditors[editorId] = quill;
};

window.getQuillHtml = (editorId) => {
    const quill = window.quillEditors[editorId];
    return quill ? quill.root.innerHTML : "";
};

window.setQuillHtml = (editorId, html) => {
    const quill = window.quillEditors[editorId];
    if (quill) {
        quill.root.innerHTML = html;
    } else {
        console.error('Quill editor not found for ID:', editorId);
    }
};

window.clearQuillEditor = (editorId) => {
    const quill = window.quillEditors[editorId];
    if (quill) {
        quill.setText("");
    }
};