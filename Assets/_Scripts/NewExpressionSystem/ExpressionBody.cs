﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpressionBody : QuickButton
{
    Expressions expression;
    Transform expressionParent;
    Transform panel;
    Transform feedBack;
    TMPro.TextMeshPro textInput;
    string title = "X";
    OutputManager outputManager;
    CalcInput calcInput;
    ParametricExpression param;
    VectorFieldExpression vec;
    ParametricManager paramManager;
    VecFieldManager vecFieldManager;
    CalculatorManager calcManager;

    private bool thisBodySelected = false;
    private bool finishedScalingUp = true;
    private bool finishedScalingDown = true;
    private bool variable = false;

    private Vector3 idleScale, selectedScale;

    private IEnumerator scaleUp, scaleDown;

    private void Awake()
    {
        paramManager = ParametricManager._instance;
        vecFieldManager = VecFieldManager._instance;
        calcManager = paramManager;

        expression = GameObject.Find("Expressions").GetComponent<Expressions>();
        feedBack = transform.parent.Find("Feedback");

        if (transform.parent.parent.Find("VariableTitle")) variable = true;

        if (!variable) title = transform.parent.Find("Title").GetComponent<TMPro.TextMeshPro>().text.Substring(0, 1);

        if (transform.parent.Find("Text_Input"))
            textInput = transform.parent.Find("Text_Input").GetComponent<TMPro.TextMeshPro>();

        outputManager = expression.GetComponent<OutputManager>();
        calcInput = CalcInput._instance;

        selectedScale = (variable) ? new Vector3(0.7f, 0.04f, 0.002f) :
                                     new Vector3(4.3f, 0.04f, 0.002f);
        idleScale = new Vector3(0f, 0.04f, 0.002f);
    }

    protected override void Start()
    {
        base.Start();
    }

    public void setExpressionParent(Transform p)
    {
        expressionParent = p;
    }

    public Transform getExpressionParent()
    {
        return expressionParent;
    }

    public void setPanel(Transform p)
    {
        panel = p;
    }

    public Transform getPanel()
    {
        return panel;
    }

    public Transform getFeedBack()
    {
        return feedBack;
    }

    public TMPro.TextMeshPro getTextInput()
    {
        return textInput;
    }

    public void setTitle(string t)
    {
        transform.parent.Find("Title").GetComponent<TMPro.TextMeshPro>().text = t;
        title = t;
    }

    public string getTitle()
    {
        return title;
    }

    public bool isVariable()
    {
        return variable;
    }

    //NOTE: sets selected expression and body to be null
    public void unSelect()
    {
        if (!finishedScalingUp)
        {
            StopCoroutine(scaleUp);
            finishedScalingUp = true;
        }

        scaleDown = ScaleTo(feedBack, feedBack.localScale, idleScale, 0.5f);
        StartCoroutine(scaleDown);
        finishedScalingDown = false;

        expression.setSelectedExpr(null, null);
        thisBodySelected = false;
    }

    public void deselectCurrBody()
    {
        if (expression == null) expression = GameObject.Find("Expressions").GetComponent<Expressions>();
        ExpressionBody selectedBody = expression.getSelectedBody();
        if (selectedBody)
        {
            TMPro.TextMeshPro oldTextInput = selectedBody.getTextInput();
            oldTextInput.text = oldTextInput.text.Replace("_", "");

            disableActionButtons(selectedBody);
            unSelect();
        }
    }

    private void disableActionButtons(ExpressionBody selectedBody)
    {
        if (selectedBody.getExpressionParent().GetComponent<ParametricExpression>())
        {
            param = selectedBody.getExpressionParent().GetComponent<ParametricExpression>();
            param.getExpActions().disableButtons();
        }
        else if (selectedBody.getExpressionParent().GetComponent<VectorFieldExpression>())
        {
            vec = selectedBody.getExpressionParent().GetComponent<VectorFieldExpression>();
            vec.getExpActions().disableButtons();
        }
    }

    private void changeExpressionSet()
    {
        if (expressionParent.GetComponent<ParametricExpression>())
        {
            param = expressionParent.GetComponent<ParametricExpression>();
            if (!paramManager) paramManager = ParametricManager._instance;
            calcManager = paramManager;
            calcManager.ChangeExpressionSet(param.getExpSet());
        }
        else if (expressionParent.GetComponent<VectorFieldExpression>())
        {
            vec = expressionParent.GetComponent<VectorFieldExpression>();
            if (!vecFieldManager) vecFieldManager = VecFieldManager._instance;
            calcManager = vecFieldManager;
            calcManager.ChangeExpressionSet(vec.getExpSet());
        }
    }

    private void selectBodyIfActive()
    {
        if (expressionParent.GetComponent<ParametricExpression>() != null)
        {
            param = expressionParent.GetComponent<ParametricExpression>();
            if (param.getActiveStatus()) selectBody();
        }
        else if (expressionParent.GetComponent<VectorFieldExpression>() != null)
        {
            vec = expressionParent.GetComponent<VectorFieldExpression>();
            if (vec.getActiveStatus()) selectBody();
        }
    }

    public void deselectPrevBody()
    {
        if (expression == null) expression = GameObject.Find("Expressions").GetComponent<Expressions>();
        ExpressionBody selectedBody = expression.getSelectedBody();
        if (selectedBody)
        {
            TMPro.TextMeshPro oldTextInput = selectedBody.getTextInput();
            oldTextInput.text = oldTextInput.text.Replace("_", "");

            disableActionButtons(selectedBody);

            if (selectedBody.transform != transform)
            {
                selectedBody.unSelect();
            }
        }
    }

    public void selectBody()
    {
        deselectPrevBody();
        expression.setSelectedExpr(expressionParent, this);
        
        changeExpressionSet();

        if (variable)
        {
            title = transform.parent.parent.Find("VariableTitle").Find("Title").GetComponent<TMPro.TextMeshPro>().text;
            outputManager.HandleInput(transform.parent.name, title);
        }
        else
        {
            if (expressionParent.GetComponent<ParametricExpression>())
            {
                param = expressionParent.GetComponent<ParametricExpression>();
                if (!paramManager) paramManager = ParametricManager._instance;
                calcManager = paramManager;
                calcManager.SetOutput(paramManager.expressionSet.GetExpression(title));
            }
            else if (expressionParent.GetComponent<VectorFieldExpression>())
            {
                vec = expressionParent.GetComponent<VectorFieldExpression>();
                if (!vecFieldManager) vecFieldManager = VecFieldManager._instance;
                calcManager = vecFieldManager;
                calcManager.SetOutput(vecFieldManager.expressionSet.GetExpression(title));
            }
        }

        if (!finishedScalingDown)
        {
            StopCoroutine(scaleDown);
            finishedScalingDown = true;
        }

        if (!feedBack) feedBack = transform.parent.Find("Feedback");
        scaleUp = ScaleTo(feedBack, feedBack.localScale, selectedScale, 0.3f);
        //BUG: couroutine won't start because ButtonInput isn't active at this point
        StartCoroutine(scaleUp);
        finishedScalingUp = false;
        thisBodySelected = true;
    }

    protected override void ButtonEnterBehavior(GameObject other)
    {
        if (thisBodySelected)
        {
            deselectCurrBody();
        }
        else
        {
            selectBodyIfActive();
        }
    }

    protected override void ButtonExitBehavior(GameObject other) { }

    IEnumerator ScaleTo(Transform obj, Vector3 start, Vector3 end, float overTime)
    {
        float startTime = Time.time;

        if (end == selectedScale) obj.gameObject.SetActive(true);

        while (Time.time < startTime + overTime)
        {
            obj.localScale = Vector3.Lerp(start, end, (Time.time - startTime) / overTime);
            yield return null;
        }

        obj.localScale = end;
        if (end == idleScale)
        {
            obj.gameObject.SetActive(false);
            finishedScalingDown = true;
        } 
        else if (end == selectedScale)
        {
            finishedScalingUp = true;
        }
    }

    private void OnDisable()
    {
        if (feedBack && feedBack.localScale == selectedScale)
        {
            feedBack.localScale = idleScale;
            feedBack.gameObject.SetActive(false);

            if (thisBodySelected)
            {
                ExpressionBody selectedBody = expression.getSelectedBody();
                TMPro.TextMeshPro oldTextInput = selectedBody.getTextInput();
                oldTextInput.text = oldTextInput.text.Replace("_", "");
                expression.setSelectedExpr(null, null);
                thisBodySelected = false;
                calcInput.ChangeOutput(null, paramManager);

                if (expressionParent.GetComponent<ParametricExpression>())
                {
                    if (!paramManager) paramManager = ParametricManager._instance;
                    calcManager = paramManager;
                    calcInput.ChangeOutput(null, paramManager);
                }
                else if (expressionParent.GetComponent<VectorFieldExpression>())
                {
                    vec = expressionParent.GetComponent<VectorFieldExpression>();
                    if (!vecFieldManager) vecFieldManager = VecFieldManager._instance;
                    calcManager = vecFieldManager;
                    calcInput.ChangeOutput(null, vecFieldManager);
                }
            }
        }
    }

    void Update() { }
}
