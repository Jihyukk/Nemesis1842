using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CocktailMakingManager : MonoBehaviour
{
    // [SerializeField]
    // private CocktailEvaluationManager _cocktailEvaluationManager;

    [SerializeField]
    private ShakerInfoUI _shakerInfoUI;

    [SerializeField]
    private Button _enterAlcoholPhaseButton;

    [SerializeField]
    private SpriteRenderer _cocktailSpriteRenderer;

    [SerializeField]
    private GameObject _enterAlcoholPhaseButtonGO;

    [SerializeField]
    private GameObject _openRecipeButtonGO;

    [SerializeField]
    private CocktailRecipeUI _cocktailRecipeUI;

    [SerializeField]
    private Image _completedCocktialImage;

    [SerializeField]
    private Image _tasteInActivateImage;
    [SerializeField]
    private Image _scentInActivateImage;

    [SerializeField]
    private PlayableDirector _handPlayableDirector;

    [SerializeField]
    private PlayableAsset[] _tasteMaterialTimeline;
    
    [SerializeField]
    private PlayableAsset[] _scentMaterialTimeline;

    private List<CocktailData> _cocktailDatas = new();

    private Cocktail.ETasteType? _tasteType;
    public Cocktail.ETasteType? TasteType => _tasteType;
    
    private Cocktail.EScentType? _scentType;
    public Cocktail.EScentType? ScentType => _scentType;

    private CocktailData _resultCocktailData;
    public CocktailData ResultCocktailData => _resultCocktailData;

    private string _cocktailCode;
    public string CocktailCode => _cocktailCode;

    private int _cocktailMistakeCount;
    public int CocktailMistakeCount => _cocktailMistakeCount;
    
    private List<CocktailRejectScriptEntity> _rejectScriptDatas;

    private void Awake()
    {
        _cocktailDatas = Resources.LoadAll<CocktailData>("GameData/CocktailData").ToList();
        ResetStepCocktail();
        
        _tasteInActivateImage.raycastTarget = false;
        _tasteInActivateImage.color = new Color32(0, 0, 0, 0);
        
        _scentInActivateImage.raycastTarget = true;
        _scentInActivateImage.color = new Color32(0, 0, 0, 233);
    }

    public IEnumerator SetTaste(Cocktail.ETasteType? tasteType)
    {
        _handPlayableDirector.playableAsset = _tasteMaterialTimeline[(int)tasteType];
        _handPlayableDirector.Play();
        _tasteType = tasteType;
        _shakerInfoUI.UpdateShakerInfoUI();
        
        _tasteInActivateImage.raycastTarget = true;
        _tasteInActivateImage.DOColor(new Color32(0, 0, 0, 233), 0.3f);

        yield return new WaitUntil(() => _handPlayableDirector.state == PlayState.Paused);

        _scentInActivateImage.DOColor(new Color32(0, 0, 0, 0), 0.3f)
            .OnKill(() =>
            {
                _scentInActivateImage.raycastTarget = false;
            });
        
        if (IsMaterialEmpty() is false)
        {
            if (TutorialManager.Inst.UseTutorial && TutorialManager.Inst.TutorialUIIndex == 9)
            {
                TutorialManager.Inst.ShowTutorial();
            }
            _enterAlcoholPhaseButton.interactable = true;
        }
    }

    public IEnumerator SetScent(Cocktail.EScentType? scentType)
    {
        _handPlayableDirector.playableAsset = _scentMaterialTimeline[(int)scentType];
        _handPlayableDirector.Play();
        
        _scentType = scentType;
        _shakerInfoUI.UpdateShakerInfoUI();
        
        _scentInActivateImage.raycastTarget = true;
        _scentInActivateImage.DOColor(new Color32(0, 0, 0, 233), 0.3f);
        
        yield return new WaitUntil(() => _handPlayableDirector.state == PlayState.Paused);

        if (IsMaterialEmpty() is false)
        {
            if (TutorialManager.Inst.UseTutorial && TutorialManager.Inst.TutorialUIIndex == 9)
            {
                TutorialManager.Inst.ShowTutorial();
            }
            _enterAlcoholPhaseButton.interactable = true;
        }
    }

    public void SetStepCocktail(string cocktailCode, List<CocktailRejectScriptEntity> rejectScriptEntities)
    {
        _rejectScriptDatas = rejectScriptEntities;
        _cocktailCode = cocktailCode;
    }

    public void ResetStepCocktail()
    {
        _enterAlcoholPhaseButton.interactable = false;
        _tasteType = null;
        _scentType = null;
        _resultCocktailData = null;
        _shakerInfoUI.ResetShakerInfoUI();
        _cocktailSpriteRenderer.color = new Color(1, 1, 1, 0);
        
        _tasteInActivateImage.raycastTarget = false;
        _tasteInActivateImage.color = new Color32(0, 0, 0, 0);
        
        _scentInActivateImage.raycastTarget = true;
        _scentInActivateImage.color = new Color32(0, 0, 0, 233);
    }

    public void ResetTurnCocktail()
    {
        _cocktailMistakeCount = 0;
    }

    public void FinishMakingCocktail()
    {
        AudioManager.Inst.PlaySFX("mouse_click");
        foreach (var cocktailData in _cocktailDatas)
        {
            if (cocktailData.TasteType == _tasteType
                && cocktailData.ScentType == _scentType)
            {
                _resultCocktailData = cocktailData;
                _cocktailSpriteRenderer.sprite = _resultCocktailData.CocktailSprite;

                if (_resultCocktailData.CocktailCode != _cocktailCode)
                {
                    _cocktailMistakeCount++;
                    _tasteType = null;
                    _scentType = null;
                    _shakerInfoUI.ResetShakerInfoUI();
                    _enterAlcoholPhaseButton.interactable = false;
                    
                    int randomIndex = Random.Range(0, _rejectScriptDatas.Count);
                    CocktailMakingScreenDialougeManager.Inst.StartDialogue(_rejectScriptDatas[randomIndex]
                        .cockail_reject_script);
                    
                    _tasteInActivateImage.DOColor(new Color32(0, 0, 0, 0), 0.3f)
                        .OnKill(() =>
                        {
                            _tasteInActivateImage.raycastTarget = false;
                        });
                }
                else
                {
                    _enterAlcoholPhaseButtonGO.SetActive(false);
                    _completedCocktialImage.sprite = _resultCocktailData.CocktailSprite;
                    _completedCocktialImage.SetNativeSize();
                    
                    var sizeDelta = _completedCocktialImage.rectTransform.sizeDelta;
                    sizeDelta = new Vector2(
                        sizeDelta.x * 2,
                        sizeDelta.y * 2);
                    _completedCocktialImage.rectTransform.sizeDelta = sizeDelta;

                    BarGameManager.Inst.OnClickEnterCutSceneButton();
                }
                
                return;
            }
        }

        _cocktailMistakeCount++;
        _tasteType = null;
        _scentType = null;
        _enterAlcoholPhaseButton.interactable = false;
        _shakerInfoUI.ResetShakerInfoUI();
                    
        int randomIndex2 = Random.Range(0, _rejectScriptDatas.Count);
        CocktailMakingScreenDialougeManager.Inst.StartDialogue(_rejectScriptDatas[randomIndex2]
            .cockail_reject_script);
        
        _tasteInActivateImage.DOColor(new Color32(0, 0, 0, 0), 0.3f)
            .OnKill(() =>
            {
                _tasteInActivateImage.raycastTarget = false;
            });
    }

    private bool IsMaterialEmpty()
    {
        return TasteType == null || ScentType == null;
    }
}
