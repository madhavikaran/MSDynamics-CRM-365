function checkAddExistingEdiblityForCustomRelationShip(formContext){
    if(formContext.ui.getFormType() === 3 || formContext.ui.getFormType() === 4){
        return true;
    }
    else{
        return false;
    }
}
