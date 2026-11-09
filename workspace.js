const controls = document.querySelector('#simulation-controls');
const approval = document.querySelector('#approval-state');
const training = document.querySelector('#training-state');
const summary = document.querySelector('#approval-summary');
const previewButton = document.querySelector('#preview-approval');
const resetButton = document.querySelector('#reset-approval');
const pendingSummary = summary.textContent;

function showApprovalPreview(approved) {
    approval.textContent = approved ? 'Approved' : 'Pending';
    approval.dataset.state = approved ? 'complete' : 'pending';
    training.textContent = approved ? 'Ready to start' : 'Blocked';
    training.dataset.state = approved ? 'ready' : 'blocked';
    summary.textContent = approved
        ? 'The approver has signed off in this preview. The training lead can now start team training.'
        : pendingSummary;

    previewButton.hidden = approved;
    resetButton.hidden = !approved;
    (approved ? resetButton : previewButton).focus();
}

previewButton.addEventListener('click', () => showApprovalPreview(true));
resetButton.addEventListener('click', () => showApprovalPreview(false));
controls.hidden = false;
