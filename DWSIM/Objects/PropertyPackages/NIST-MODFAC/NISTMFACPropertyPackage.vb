﻿'    Modified UNIFAC (NIST) Property Package 
'    Copyright 2015 Daniel Wagner O. de Medeiros
'    Copyright 2015 Gregor Reichert
'
'    Based on the paper entitled "New modified UNIFAC parameters using critically 
'    evaluated phase equilibrium data", http://dx.doi.org/10.1016/j.fluid.2014.12.042
'
'    This file is part of DWSIM.
'
'    DWSIM is free software: you can redistribute it and/or modify
'    it under the terms of the GNU General Public License as published by
'    the Free Software Foundation, either version 3 of the License, or
'    (at your option) any later version.
'
'    DWSIM is distributed in the hope that it will be useful,
'    but WITHOUT ANY WARRANTY; without even the implied warranty of
'    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    GNU General Public License for more details.
'
'    You should have received a copy of the GNU General Public License
'    along with DWSIM.  If not, see <http://www.gnu.org/licenses/>.

Imports System.Math
Imports DTL.DTL.BaseThermoClasses

Namespace DTL.SimulationObjects.PropertyPackages

    <Runtime.InteropServices.Guid(NISTMFACPropertyPackage.ClassId)> _
    <Serializable()> Public Class NISTMFACPropertyPackage

        Inherits PropertyPackage

        Public Shadows Const ClassId As String = "519EB917-0B2E-4ac1-9AF2-2D1A2A55067F"

        Public MAT_KIJ(38, 38)

        Private m_props As New Auxiliary.PROPS
        Public m_pr As New Auxiliary.PengRobinson
        Private m_uni As New Auxiliary.NISTMFAC
        Private m_lk As New Auxiliary.LeeKesler

        Public Sub New(ByVal comode As Boolean)
            MyBase.New(comode)
            With Me.Parameters
                .Add("PP_IDEAL_VAPOR_PHASE_FUG", 1)
                .Add("PP_ENTH_CP_CALC_METHOD", 1)
                .Item("PP_IDEAL_MIXRULE_LIQDENS") = 1
            End With
        End Sub

        Public Sub New()

            MyBase.New()

            With Me.Parameters
                .Add("PP_IDEAL_VAPOR_PHASE_FUG", 1)
                .Add("PP_ENTH_CP_CALC_METHOD", 1)
                .Item("PP_IDEAL_MIXRULE_LIQDENS") = 1
                .Item("PP_USEEXPLIQDENS") = 1
            End With

            Me.IsConfigurable = True
            Me._packagetype = PackageType.ActivityCoefficient

        End Sub

        Public Overrides Sub ReconfigureConfigForm()
            MyBase.ReconfigureConfigForm()
        End Sub

#Region "DWSIM Functions"

        Public Function RET_KIJ(ByVal id1 As String, ByVal id2 As String) As Double
            If Me.m_pr.InteractionParameters.ContainsKey(id1) Then
                If Me.m_pr.InteractionParameters(id1).ContainsKey(id2) Then
                    Return m_pr.InteractionParameters(id1)(id2).kij
                Else
                    If Me.m_pr.InteractionParameters.ContainsKey(id2) Then
                        If Me.m_pr.InteractionParameters(id2).ContainsKey(id1) Then
                            Return m_pr.InteractionParameters(id2)(id1).kij
                        Else
                            Return 0
                        End If
                    Else
                        Return 0
                    End If
                End If
            Else
                Return 0
            End If
        End Function

        Public Overrides Function RET_VKij() As Double(,)

            Dim val(Me.CurrentMaterialStream.Phases(0).Components.Count - 1, Me.CurrentMaterialStream.Phases(0).Components.Count - 1) As Double
            Dim i As Integer = 0
            Dim l As Integer = 0

            i = 0
            For Each cp As Substance In Me.CurrentMaterialStream.Phases(0).Components.Values
                l = 0
                For Each cp2 As Substance In Me.CurrentMaterialStream.Phases(0).Components.Values
                    val(i, l) = Me.RET_KIJ(cp.Name, cp2.Name)
                    l = l + 1
                Next
                i = i + 1
            Next

            Return val

        End Function

        Public Overrides Function DW_CalcCp_ISOL(ByVal Phase1 As Phase, ByVal T As Double, ByVal P As Double) As Double
            Select Case Phase1
                Case Phase.Liquid
                    Return Me.m_props.CpCvR("L", T, P, RET_VMOL(Phase1), RET_VKij(), RET_VMAS(Phase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(1)
                Case Phase.Aqueous
                    Return Me.m_props.CpCvR("L", T, P, RET_VMOL(Phase1), RET_VKij(), RET_VMAS(Phase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(1)
                Case Phase.Liquid1
                    Return Me.m_props.CpCvR("L", T, P, RET_VMOL(Phase1), RET_VKij(), RET_VMAS(Phase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(1)
                Case Phase.Liquid2
                    Return Me.m_props.CpCvR("L", T, P, RET_VMOL(Phase1), RET_VKij(), RET_VMAS(Phase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(1)
                Case Phase.Liquid3
                    Return Me.m_props.CpCvR("L", T, P, RET_VMOL(Phase1), RET_VKij(), RET_VMAS(Phase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(1)
                Case Phase.Vapor
                    Return Me.m_props.CpCvR("V", T, P, RET_VMOL(Phase1), RET_VKij(), RET_VMAS(Phase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(1)
            End Select
        End Function

        Public Overrides Function DW_CalcMixtureEnergy_ISOL(ByVal T As Double, ByVal P As Double) As Double

            Dim HM, HV, HL As Double

            HL = Me.m_lk.H_LK_MIX("L", T, P, RET_VMOL(Phase.Liquid), RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Phase.Liquid))
            HV = Me.m_lk.H_LK_MIX("V", T, P, RET_VMOL(Phase.Vapor), RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Phase.Vapor))
            HM = Me.CurrentMaterialStream.Phases(1).SPMProperties.massfraction.GetValueOrDefault * HL + Me.CurrentMaterialStream.Phases(2).SPMProperties.massfraction.GetValueOrDefault * HV

            Dim ent_mass = HM
            Dim flow = Me.CurrentMaterialStream.Phases(0).SPMProperties.massflow
            Return ent_mass * flow

        End Function

        Public Overrides Function DW_CalcK_ISOL(ByVal Phase1 As Phase, ByVal T As Double, ByVal P As Double) As Double
            If Phase1 = Phase.Liquid Then
                Return Me.AUX_CONDTL(T)
            ElseIf Phase1 = Phase.Vapor Then
                Return Me.AUX_CONDTG(T, P)
            End If
        End Function

        Public Overrides Function DW_CalcSpecificMass_ISOL(ByVal Phase1 As Phase, ByVal T As Double, ByVal P As Double, Optional ByVal pvp As Double = 0) As Double
            If Phase1 = Phase.Liquid Then
                Return Me.AUX_LIQDENS(T)
            ElseIf Phase1 = Phase.Vapor Then
                Return Me.AUX_VAPDENS(T, P)
            ElseIf Phase1 = Phase.Mixture Then
                Return Me.CurrentMaterialStream.Phases(1).SPMProperties.volumetric_flow.GetValueOrDefault * Me.AUX_LIQDENS(T) / Me.CurrentMaterialStream.Phases(0).SPMProperties.volumetric_flow.GetValueOrDefault + Me.CurrentMaterialStream.Phases(2).SPMProperties.volumetric_flow.GetValueOrDefault * Me.AUX_VAPDENS(T, P) / Me.CurrentMaterialStream.Phases(0).SPMProperties.volumetric_flow.GetValueOrDefault
            End If
        End Function

        Public Overrides Function DW_CalcMM_ISOL(ByVal Phase1 As Phase, ByVal T As Double, ByVal P As Double) As Double
            Return Me.AUX_MMM(Phase1)
        End Function

        Public Overrides Sub DW_CalcOverallProps()
            MyBase.DW_CalcOverallProps()
        End Sub

        Public Overrides Sub DW_CalcProp(ByVal [property] As String, ByVal phase As Phase)

            Dim result As Double = 0.0#
            Dim resultObj As Object = Nothing
            Dim phaseID As Integer = -1
            Dim state As String = ""

            Dim T, P As Double
            T = Me.CurrentMaterialStream.Phases(0).SPMProperties.temperature.GetValueOrDefault
            P = Me.CurrentMaterialStream.Phases(0).SPMProperties.pressure.GetValueOrDefault

            Select Case phase
                Case Phase.Vapor
                    state = "V"
                Case Phase.Liquid, Phase.Liquid1, Phase.Liquid2, Phase.Liquid3
                    state = "L"
            End Select

            Select Case phase
                Case Phase.Mixture
                    phaseID = 0
                Case Phase.Vapor
                    phaseID = 2
                Case Phase.Liquid1
                    phaseID = 3
                Case Phase.Liquid2
                    phaseID = 4
                Case Phase.Liquid3
                    phaseID = 5
                Case Phase.Liquid
                    phaseID = 1
                Case Phase.Aqueous
                    phaseID = 6
                Case Phase.Solid
                    phaseID = 7
            End Select

            Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molecularWeight = Me.AUX_MMM(phase)

            Select Case [property].ToLower
                Case "compressibilityfactor"
                    result = Me.m_lk.Z_LK(state, T / Me.AUX_TCM(phase), P / Me.AUX_PCM(phase), Me.AUX_WM(phase))(0)
                    Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.compressibilityFactor = result
                Case "heatcapacity", "heatcapacitycp"
                    If state = "V" Then
                        resultObj = Me.m_lk.CpCvR_LK(state, T, P, RET_VMOL(phase), RET_VKij(), RET_VMAS(phase), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
                        result = resultObj(1)
                    Else
                        Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                            Case 0 'LK
                                resultObj = Me.m_lk.CpCvR_LK(state, T, P, RET_VMOL(phase), RET_VKij(), RET_VMAS(phase), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
                                result = resultObj(1)
                            Case 1 'Ideal
                                result = Me.AUX_LIQCPm(T, phaseID)
                            Case 2 'Excess
                                result = Me.AUX_LIQCPm(T, phaseID) + Me.m_uni.CPEX_MIX(T, RET_VMOL(phase), Me.RET_VQ, Me.RET_VR, Me.RET_VEKI) / Me.AUX_MMM(phase)
                        End Select
                    End If
                    Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.heatCapacityCp = result
                Case "heatcapacitycv"
                    If state = "V" Then
                        resultObj = Me.m_lk.CpCvR_LK(state, T, P, RET_VMOL(phase), RET_VKij(), RET_VMAS(phase), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
                        result = resultObj(2)
                    Else
                        Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                            Case 0 'LK
                                resultObj = Me.m_lk.CpCvR_LK(state, T, P, RET_VMOL(phase), RET_VKij(), RET_VMAS(phase), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
                                result = resultObj(2)
                            Case 1 'Ideal
                                result = Me.AUX_LIQCPm(T, phaseID)
                            Case 2 'Excess
                                result = Me.AUX_LIQCPm(T, phaseID) + Me.m_uni.CPEX_MIX(T, RET_VMOL(phase), Me.RET_VQ, Me.RET_VR, Me.RET_VEKI) / Me.AUX_MMM(phase)
                        End Select
                    End If
                    Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.heatCapacityCv = result
                Case "enthalpy", "enthalpynf"
                    If state = "V" Then
                        result = Me.m_lk.H_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Hid(298.15, T, phase))
                    Else
                        Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                            Case 0 'LK
                                result = Me.m_lk.H_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Hid(298.15, T, phase))
                            Case 1 'Ideal
                                result = Me.RET_Hid_L(298.15, T, RET_VMOL(phase)) - Me.RET_HVAPM(RET_VMAS(phase), T)
                            Case 2 'Excess
                                result = Me.RET_Hid_L(298.15, T, RET_VMOL(phase)) + Me.m_uni.HEX_MIX(T, RET_VMOL(phase), Me.RET_VQ, Me.RET_VR, Me.RET_VEKI) / Me.AUX_MMM(phase) - Me.RET_HVAPM(RET_VMAS(phase), T)
                        End Select
                    End If
                    Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.enthalpy = result
                    result = Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.enthalpy.GetValueOrDefault * Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                    Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molar_enthalpy = result
                Case "entropy", "entropynf"
                    If state = "V" Then
                        result = Me.m_lk.S_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Sid(298.15, T, P, phase))
                    Else
                        Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                            Case 0 'LK
                                result = Me.m_lk.S_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Sid(298.15, T, P, phase))
                            Case 1 'Ideal
                                result = Me.RET_Hid_L(298.15, T, RET_VMOL(phase)) / T - Me.RET_HVAPM(RET_VMAS(phase), T) / T
                            Case 2 'Excess
                                result = (Me.RET_Hid_L(298.15, T, RET_VMOL(phase)) + Me.m_uni.HEX_MIX(T, RET_VMOL(phase), Me.RET_VQ, Me.RET_VR, Me.RET_VEKI)) / Me.AUX_MMM(phase) / T - Me.RET_HVAPM(RET_VMAS(phase), T) / T
                        End Select
                    End If
                    Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.entropy = result
                    result = Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.entropy.GetValueOrDefault * Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                    Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molar_entropy = result
                Case "excessenthalpy"
                    If state = "V" Then
                        result = Me.m_lk.H_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), 0)
                    Else
                        Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                            Case 0 'LK
                                result = Me.m_lk.H_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), 0)
                            Case 1 'Ideal
                                result = 0.0#
                            Case 2 'Excess
                                result = Me.m_uni.HEX_MIX(T, RET_VMOL(phase), Me.RET_VQ, Me.RET_VR, Me.RET_VEKI) / Me.AUX_MMM(phase)
                        End Select
                    End If
                    Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.excessEnthalpy = result
                Case "excessentropy"
                    If state = "V" Then
                        result = Me.m_lk.S_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), 0)
                    Else
                        Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                            Case 0 'LK
                                result = Me.m_lk.S_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), 0)
                            Case 1 'Ideal
                                result = 0.0#
                            Case 2 'Excess
                                result = Me.m_uni.HEX_MIX(T, RET_VMOL(phase), Me.RET_VQ, Me.RET_VR, Me.RET_VEKI) / Me.AUX_MMM(phase) / T
                        End Select
                    End If
                    Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.excessEntropy = result
                Case "enthalpyf"
                    Dim entF As Double = Me.AUX_HFm25(phase)
                    result = Me.m_lk.H_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Hid(298.15, T, phase))
                    Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.enthalpyF = result + entF
                    result = Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.enthalpyF.GetValueOrDefault * Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                    Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molar_enthalpyF = result
                Case "entropyf"
                    Dim entF As Double = Me.AUX_SFm25(phase)
                    result = Me.m_lk.S_LK_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Sid(298.15, T, P, phase))
                    Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.entropyF = result + entF
                    result = Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.entropyF.GetValueOrDefault * Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                    Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molar_entropyF = result
                Case "viscosity"
                    If state = "L" Then
                        result = Me.AUX_LIQVISCm(T)
                    Else
                        result = Me.AUX_VAPVISCm(T, Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.density.GetValueOrDefault, Me.AUX_MMM(phase))
                    End If
                    Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.viscosity = result
                Case "thermalconductivity"
                    If state = "L" Then
                        result = Me.AUX_CONDTL(T)
                    Else
                        result = Me.AUX_CONDTG(T, P)
                    End If
                    Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.thermalConductivity = result
                Case "fugacity", "fugacitycoefficient", "logfugacitycoefficient", "activity", "activitycoefficient"
                    Me.DW_CalcCompFugCoeff(phase)
                Case "volume", "density"
                    If state = "L" Then
                        result = Me.AUX_LIQDENS(T, P, 0.0#, phaseID, False)
                    Else
                        result = Me.AUX_VAPDENS(T, P)
                    End If
                    Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.density = result
                Case "surfacetension"
                    Me.CurrentMaterialStream.Phases(0).TPMProperties.surfaceTension = Me.AUX_SURFTM(T)
            End Select

        End Sub

        Public Overrides Sub DW_CalcPhaseProps(ByVal Phase As Phase)

            Dim result As Double
            Dim resultObj As Object
            Dim dwpl As Phase

            Dim T, P As Double
            Dim phasemolarfrac As Double = Nothing
            Dim overallmolarflow As Double = Nothing

            Dim phaseID As Integer
            T = Me.CurrentMaterialStream.Phases(0).SPMProperties.temperature.GetValueOrDefault
            P = Me.CurrentMaterialStream.Phases(0).SPMProperties.pressure.GetValueOrDefault

            Select Case Phase
                Case Phase.Mixture
                    phaseID = 0
                    dwpl = Phase.Mixture
                Case Phase.Vapor
                    phaseID = 2
                    dwpl = Phase.Vapor
                Case Phase.Liquid1
                    phaseID = 3
                    dwpl = Phase.Liquid1
                Case Phase.Liquid2
                    phaseID = 4
                    dwpl = Phase.Liquid2
                Case Phase.Liquid3
                    phaseID = 5
                    dwpl = Phase.Liquid3
                Case Phase.Liquid
                    phaseID = 1
                    dwpl = Phase.Liquid
                Case Phase.Aqueous
                    phaseID = 6
                    dwpl = Phase.Aqueous
                Case Phase.Solid
                    phaseID = 7
                    dwpl = Phase.Solid
            End Select

            If phaseID > 0 Then
                overallmolarflow = Me.CurrentMaterialStream.Phases(0).SPMProperties.molarflow.GetValueOrDefault
                phasemolarfrac = Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molarfraction.GetValueOrDefault
                result = overallmolarflow * phasemolarfrac
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molarflow = result
                result = result * Me.AUX_MMM(Phase) / 1000
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.massflow = result
                result = phasemolarfrac * overallmolarflow * Me.AUX_MMM(Phase) / 1000 / Me.CurrentMaterialStream.Phases(0).SPMProperties.massflow.GetValueOrDefault
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.massfraction = result
                Me.DW_CalcCompVolFlow(phaseID)
                Me.DW_CalcCompFugCoeff(Phase)
            End If

            If phaseID = 3 Or phaseID = 4 Or phaseID = 5 Or phaseID = 6 Then

                result = Me.AUX_LIQDENS(T, P, 0.0#, phaseID, False)
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.density = result
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.enthalpy = Me.DW_CalcEnthalpy(RET_VMOL(dwpl), T, P, State.Liquid)
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.entropy = Me.DW_CalcEntropy(RET_VMOL(dwpl), T, P, State.Liquid)
                result = Me.m_lk.Z_LK("L", T / Me.AUX_TCM(dwpl), P / Me.AUX_PCM(dwpl), Me.AUX_WM(dwpl))(0)
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.compressibilityFactor = result
                Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                    Case 0 'LK
                        resultObj = Me.m_lk.CpCvR_LK("L", T, P, RET_VMOL(dwpl), RET_VKij(), RET_VMAS(dwpl), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
                        Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.heatCapacityCp = resultObj(1)
                        Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.heatCapacityCv = resultObj(2)
                    Case 1 'Ideal
                        result = Me.AUX_LIQCPm(T, phaseID)
                        Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.heatCapacityCp = result
                        Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.heatCapacityCv = result
                    Case 2 'Excess
                        result = Me.AUX_LIQCPm(T, phaseID) + Me.m_uni.CPEX_MIX(T, RET_VMOL(dwpl), Me.RET_VQ, Me.RET_VR, Me.RET_VEKI) / Me.AUX_MMM(dwpl)
                        Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.heatCapacityCp = result
                        Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.heatCapacityCv = result
                End Select
                result = Me.AUX_MMM(Phase)
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molecularWeight = result
                result = Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.enthalpy.GetValueOrDefault * Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molar_enthalpy = result
                result = Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.entropy.GetValueOrDefault * Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molar_entropy = result
                result = Me.AUX_CONDTL(T)
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.thermalConductivity = result
                result = Me.AUX_LIQVISCm(T)
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.viscosity = result
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.kinematic_viscosity = result / Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.density.Value

            ElseIf phaseID = 2 Then

                result = Me.AUX_VAPDENS(T, P)
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.density = result
                result = Me.m_lk.H_LK_MIX("V", T, P, RET_VMOL(Phase.Vapor), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Hid(298.15, T, Phase.Vapor))
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.enthalpy = result
                result = Me.m_lk.S_LK_MIX("V", T, P, RET_VMOL(Phase.Vapor), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Sid(298.15, T, P, Phase.Vapor))
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.entropy = result
                result = Me.m_lk.Z_LK("V", T / Me.AUX_TCM(Phase.Vapor), P / Me.AUX_PCM(Phase.Vapor), Me.AUX_WM(Phase.Vapor))(0)
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.compressibilityFactor = result
                result = Me.AUX_CPm(Phase.Vapor, T)
                resultObj = Me.m_lk.CpCvR_LK("V", T, P, RET_VMOL(Phase.Vapor), RET_VKij(), RET_VMAS(Phase.Vapor), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.heatCapacityCp = resultObj(1)
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.heatCapacityCv = resultObj(2)
                result = Me.AUX_MMM(Phase)
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molecularWeight = result
                result = Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.enthalpy.GetValueOrDefault * Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molar_enthalpy = result
                result = Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.entropy.GetValueOrDefault * Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molar_entropy = result
                result = Me.AUX_CONDTG(T, P)
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.thermalConductivity = result
                result = Me.AUX_VAPVISCm(T, Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.density.GetValueOrDefault, Me.AUX_MMM(Phase))
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.viscosity = result
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.kinematic_viscosity = result / Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.density.Value

            ElseIf phaseID = 7 Then

                result = Me.AUX_SOLIDDENS
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.density = result
                Dim constprops As New List(Of ConstantProperties)
                For Each su As Substance In Me.CurrentMaterialStream.Phases(0).Components.Values
                    constprops.Add(su.ConstantProperties)
                Next
                result = Me.DW_CalcSolidEnthalpy(T, RET_VMOL(Phase.Solid), constprops)
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.enthalpy = result
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.entropy = result / T
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.compressibilityFactor = 0.0# 'result
                result = Me.DW_CalcSolidHeatCapacityCp(T, RET_VMOL(Phase.Solid), constprops)
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.heatCapacityCp = result
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.heatCapacityCv = result
                result = Me.AUX_MMM(Phase)
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molecularWeight = result
                result = Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.enthalpy.GetValueOrDefault * Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molar_enthalpy = result
                result = Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.entropy.GetValueOrDefault * Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.molar_entropy = result
                result = Me.AUX_CONDTG(T, P)
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.thermalConductivity = 0.0# 'result
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.viscosity = 1.0E+20
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.kinematic_viscosity = 1.0E+20

            ElseIf phaseID = 1 Then

                DW_CalcLiqMixtureProps()

            Else

                DW_CalcOverallProps()

            End If

            If phaseID > 0 Then
                result = overallmolarflow * phasemolarfrac * Me.AUX_MMM(Phase) / 1000 / Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.density.GetValueOrDefault
                Me.CurrentMaterialStream.Phases(phaseID).SPMProperties.volumetric_flow = result
            End If

        End Sub

        Public Overrides Function DW_CalcPVAP_ISOL(ByVal T As Double) As Double
            Return Me.m_props.Pvp_leekesler(T, Me.RET_VTC(Phase.Liquid), Me.RET_VPC(Phase.Liquid), Me.RET_VW(Phase.Liquid))
        End Function

        Public Overrides Sub DW_CalcTwoPhaseProps(ByVal Phase1 As Phase, ByVal Phase2 As Phase)

            Dim T As Double

            T = Me.CurrentMaterialStream.Phases(0).SPMProperties.temperature.GetValueOrDefault
            Me.CurrentMaterialStream.Phases(0).TPMProperties.surfaceTension = Me.AUX_SURFTM(T)

        End Sub

        Public Overrides Sub DW_CalcMassFlow()
            With Me.CurrentMaterialStream
                .Phases(0).SPMProperties.massflow = .Phases(0).SPMProperties.molarflow.GetValueOrDefault * Me.AUX_MMM(Phase.Mixture) / 1000
            End With
        End Sub

        Public Overrides Sub DW_CalcMolarFlow()
            With Me.CurrentMaterialStream
                .Phases(0).SPMProperties.molarflow = .Phases(0).SPMProperties.massflow.GetValueOrDefault / Me.AUX_MMM(Phase.Mixture) * 1000
            End With
        End Sub

        Public Overrides Sub DW_CalcVolumetricFlow()
            With Me.CurrentMaterialStream
                .Phases(0).SPMProperties.volumetric_flow = .Phases(0).SPMProperties.massflow.GetValueOrDefault / .Phases(0).SPMProperties.density.GetValueOrDefault
            End With
        End Sub

        Public Overrides Function DW_CalcDynamicViscosity_ISOL(ByVal Phase1 As Phase, ByVal T As Double, ByVal P As Double) As Double
            If Phase1 = Phase.Liquid Then
                Return Me.AUX_LIQVISCm(T)
            ElseIf Phase1 = Phase.Vapor Then
                Return Me.AUX_VAPVISCm(T, Me.AUX_VAPDENS(T, P), Me.AUX_MMM(Phase.Vapor))
            End If
        End Function
        Public Overrides Function SupportsComponent(ByVal comp As ConstantProperties) As Boolean

            If Me.SupportedComponents.Contains(comp.ID) Then
                Return True
            ElseIf comp.IsHYPO = 1 Then
                Return True
            Else
                Return False
            End If

        End Function

        Function dfidRbb_H(ByVal Rbb, ByVal Kb0, ByVal Vz, ByVal Vu, ByVal sum_Hvi0, ByVal DHv, ByVal DHl, ByVal HT) As Double

            Dim i As Integer = 0
            Dim n = Vz.Length - 1

            Dim Vpbb2(n), L2, V2, Kb2 As Double

            i = 0
            Dim sum_pi2 = 0.0#
            Dim sum_eui_pi2 = 0.0#
            Do
                Vpbb2(i) = Vz(i) / (1 - Rbb + Kb0 * Rbb * Exp(Vu(i)))
                sum_pi2 += Vpbb2(i)
                sum_eui_pi2 += Exp(Vu(i)) * Vpbb2(i)
                i = i + 1
            Loop Until i = n + 1
            Kb2 = sum_pi2 / sum_eui_pi2

            L2 = (1 - Rbb) * sum_pi2
            V2 = 1 - L2

            Return L2 * (DHv - DHl) - sum_Hvi0 - DHv + HT * Me.AUX_MMM(Vz)

        End Function

        Function dfidRbb_S(ByVal Rbb, ByVal Kb0, ByVal Vz, ByVal Vu, ByVal sum_Hvi0, ByVal DHv, ByVal DHl, ByVal ST) As Double

            Dim i As Integer = 0
            Dim n = Vz.Length - 1

            Dim Vpbb2(n), L, V As Double

            i = 0
            Dim sum_pi2 = 0.0#
            Dim sum_eui_pi2 = 0.0#
            Do
                Vpbb2(i) = Vz(i) / (1 - Rbb + Kb0 * Rbb * Exp(Vu(i)))
                sum_pi2 += Vpbb2(i)
                sum_eui_pi2 += Exp(Vu(i)) * Vpbb2(i)
                i = i + 1
            Loop Until i = n + 1

            L = (1 - Rbb) * sum_pi2
            V = 1 - L

            Return L * (DHv - DHl) - sum_Hvi0 - DHv + ST * Me.AUX_MMM(Vz)

        End Function

#End Region

#Region "Auxiliary Functions"

        Function RET_VN(ByVal subst As Substance) As Object

            Return Me.m_uni.RET_VN(subst.ConstantProperties)

        End Function

        Function RET_VQ() As Object

            Dim subst As Substance
            Dim VQ(Me.CurrentMaterialStream.Phases(0).Components.Count - 1) As Double
            Dim i As Integer = 0
            Dim sum As Double = 0

            For Each subst In Me.CurrentMaterialStream.Phases(0).Components.Values
                VQ(i) = Me.m_uni.RET_Qi(Me.RET_VN(subst))
                i += 1
            Next

            Return VQ

        End Function

        Function RET_VR() As Object

            Dim subst As Substance
            Dim VR(Me.CurrentMaterialStream.Phases(0).Components.Count - 1) As Double
            Dim i As Integer = 0
            Dim sum As Double = 0

            For Each subst In Me.CurrentMaterialStream.Phases(0).Components.Values
                VR(i) = Me.m_uni.RET_Ri(Me.RET_VN(subst))
                i += 1
            Next

            Return VR

        End Function

        Function RET_VEKI() As List(Of Dictionary(Of Integer, Double))

            Dim subst As Substance
            Dim VEKI As New List(Of Dictionary(Of Integer, Double))
            Dim i As Integer = 0
            Dim sum As Double
            For Each subst In Me.CurrentMaterialStream.Phases(0).Components.Values
                sum = 0
                If subst.ConstantProperties.NISTMODFACGroups.Collection.Count > 0 Then
                    For Each s In subst.ConstantProperties.NISTMODFACGroups.Collection.Keys
                        sum += subst.ConstantProperties.NISTMODFACGroups.Collection(s) * Me.m_uni.ModfGroups.Groups(s).Q
                    Next
                Else
                    For Each s In subst.ConstantProperties.MODFACGroups.Collection.Keys
                        sum += subst.ConstantProperties.MODFACGroups.Collection(s) * Me.m_uni.ModfGroups.Groups(s).Q
                    Next
                End If
                Dim obj = Me.m_uni.RET_EKI(Me.RET_VN(subst), sum)
                VEKI.Add(obj)
            Next

            Return VEKI

        End Function

#End Region

#Region "Numerical Methods"

        Public Function IntegralSimpsonCp(ByVal a As Double,
                 ByVal b As Double,
                 ByVal Epsilon As Double, ByVal subst As String) As Double

            Dim Result As Double
            Dim switch As Boolean = False
            Dim h As Double
            Dim s As Double
            Dim s1 As Double
            Dim s2 As Double
            Dim s3 As Double
            Dim x As Double
            Dim tm As Double

            If a > b Then
                switch = True
                tm = a
                a = b
                b = tm
            ElseIf Abs(a - b) < 0.01 Then
                Return 0
            End If

            s2 = 1.0#
            h = b - a
            s = Me.AUX_CPi(subst, a) + Me.AUX_CPi(subst, b)
            Do
                s3 = s2
                h = h / 2.0#
                s1 = 0.0#
                x = a + h
                Do
                    s1 = s1 + 2.0# * Me.AUX_CPi(subst, x)
                    x = x + 2.0# * h
                Loop Until Not x < b
                s = s + s1
                s2 = (s + s1) * h / 3.0#
                x = Abs(s3 - s2) / 15.0#
            Loop Until Not x > Epsilon
            Result = s2

            If switch Then Result = -Result

            IntegralSimpsonCp = Result

        End Function

        Public Function IntegralSimpsonCp_T(ByVal a As Double,
         ByVal b As Double,
         ByVal Epsilon As Double, ByVal subst As String) As Double

            'Cp = A + B*T + C*T^2 + D*T^3 + E*T^4 where Cp in kJ/kg-mol , T in K 

            Dim Result As Double
            Dim h As Double
            Dim s As Double
            Dim s1 As Double
            Dim s2 As Double
            Dim s3 As Double
            Dim x As Double
            Dim tm As Double
            Dim switch As Boolean = False

            If a > b Then
                switch = True
                tm = a
                a = b
                b = tm
            ElseIf Abs(a - b) < 0.01 Then
                Return 0
            End If

            s2 = 1.0#
            h = b - a
            s = Me.AUX_CPi(subst, a) / a + Me.AUX_CPi(subst, b) / b
            Do
                s3 = s2
                h = h / 2.0#
                s1 = 0.0#
                x = a + h
                Do
                    s1 = s1 + 2.0# * Me.AUX_CPi(subst, x) / x
                    x = x + 2.0# * h
                Loop Until Not x < b
                s = s + s1
                s2 = (s + s1) * h / 3.0#
                x = Abs(s3 - s2) / 15.0#
            Loop Until Not x > Epsilon
            Result = s2

            If switch Then Result = -Result

            IntegralSimpsonCp_T = Result

        End Function

#End Region

        Public Overrides Function DW_CalcEnthalpy(ByVal Vx As Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double

            Dim H As Double

            If st = State.Liquid Then
                Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                    Case 0 'LK
                        H = Me.m_lk.H_LK_MIX("L", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Vx))
                    Case 1 'Ideal
                        H = Me.RET_Hid_L(298.15, T, Vx) - Me.RET_HVAPM(Me.AUX_CONVERT_MOL_TO_MASS(Vx), T)
                    Case 2 'Excess
                        H = Me.RET_Hid_L(298.15, T, Vx) + Me.m_uni.HEX_MIX(T, Vx, Me.RET_VQ, Me.RET_VR, Me.RET_VEKI) / Me.AUX_MMM(Vx) - Me.RET_HVAPM(Me.AUX_CONVERT_MOL_TO_MASS(Vx), T)
                End Select
            Else
                H = Me.m_lk.H_LK_MIX("V", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Hid(298.15, T, Vx))
            End If

            Return H

        End Function

        Public Overrides Function DW_CalcEnthalpyDeparture(ByVal Vx As Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double
            Dim H As Double

            If st = State.Liquid Then
                Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                    Case 0 'LK
                        H = Me.m_lk.H_LK_MIX("L", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, 0)
                    Case 1 'Ideal
                        H = -Me.RET_HVAPM(Me.AUX_CONVERT_MOL_TO_MASS(Vx), T)
                    Case 2 'Excess
                        H = Me.m_uni.HEX_MIX(T, Vx, Me.RET_VQ, Me.RET_VR, Me.RET_VEKI) / Me.AUX_MMM(Vx) - Me.RET_HVAPM(Me.AUX_CONVERT_MOL_TO_MASS(Vx), T)
                End Select
            Else
                H = Me.m_lk.H_LK_MIX("V", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, 0)
            End If

            Return H

        End Function

        Public Overrides Function DW_CalcCv_ISOL(ByVal Phase1 As Phase, ByVal T As Double, ByVal P As Double) As Double
            Select Case Phase1
                Case Phase.Liquid
                    Return Me.m_props.CpCvR("L", T, P, RET_VMOL(Phase1), RET_VKij(), RET_VMAS(Phase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(2)
                Case Phase.Aqueous
                    Return Me.m_props.CpCvR("L", T, P, RET_VMOL(Phase1), RET_VKij(), RET_VMAS(Phase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(2)
                Case Phase.Liquid1
                    Return Me.m_props.CpCvR("L", T, P, RET_VMOL(Phase1), RET_VKij(), RET_VMAS(Phase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(2)
                Case Phase.Liquid2
                    Return Me.m_props.CpCvR("L", T, P, RET_VMOL(Phase1), RET_VKij(), RET_VMAS(Phase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(2)
                Case Phase.Liquid3
                    Return Me.m_props.CpCvR("L", T, P, RET_VMOL(Phase1), RET_VKij(), RET_VMAS(Phase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(2)
                Case Phase.Vapor
                    Return Me.m_props.CpCvR("V", T, P, RET_VMOL(Phase1), RET_VKij(), RET_VMAS(Phase1), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())(2)
            End Select
        End Function

        Public Overrides Sub DW_CalcCompPartialVolume(ByVal phase As Phase, ByVal T As Double, ByVal P As Double)
            Select Case phase
                Case Phase.Liquid
                    For Each subst As Substance In Me.CurrentMaterialStream.Phases(1).Components.Values
                        subst.PartialVolume = 1 / 1000 * subst.ConstantProperties.Molar_Weight / Me.m_props.liq_dens_rackett(T, subst.ConstantProperties.Critical_Temperature, subst.ConstantProperties.Critical_Pressure, subst.ConstantProperties.Acentric_Factor, subst.ConstantProperties.Molar_Weight, subst.ConstantProperties.Z_Rackett, P, Me.AUX_PVAPi(subst.Name, T))
                    Next
                Case Phase.Aqueous
                    For Each subst As Substance In Me.CurrentMaterialStream.Phases(6).Components.Values
                        subst.PartialVolume = 1 / 1000 * subst.ConstantProperties.Molar_Weight / Me.m_props.liq_dens_rackett(T, subst.ConstantProperties.Critical_Temperature, subst.ConstantProperties.Critical_Pressure, subst.ConstantProperties.Acentric_Factor, subst.ConstantProperties.Molar_Weight, subst.ConstantProperties.Z_Rackett, P, Me.AUX_PVAPi(subst.Name, T))
                    Next
                Case Phase.Liquid1
                    For Each subst As Substance In Me.CurrentMaterialStream.Phases(3).Components.Values
                        subst.PartialVolume = 1 / 1000 * subst.ConstantProperties.Molar_Weight / Me.m_props.liq_dens_rackett(T, subst.ConstantProperties.Critical_Temperature, subst.ConstantProperties.Critical_Pressure, subst.ConstantProperties.Acentric_Factor, subst.ConstantProperties.Molar_Weight, subst.ConstantProperties.Z_Rackett, P, Me.AUX_PVAPi(subst.Name, T))
                    Next
                Case Phase.Liquid2
                    For Each subst As Substance In Me.CurrentMaterialStream.Phases(4).Components.Values
                        subst.PartialVolume = 1 / 1000 * subst.ConstantProperties.Molar_Weight / Me.m_props.liq_dens_rackett(T, subst.ConstantProperties.Critical_Temperature, subst.ConstantProperties.Critical_Pressure, subst.ConstantProperties.Acentric_Factor, subst.ConstantProperties.Molar_Weight, subst.ConstantProperties.Z_Rackett, P, Me.AUX_PVAPi(subst.Name, T))
                    Next
                Case Phase.Liquid3
                    For Each subst As Substance In Me.CurrentMaterialStream.Phases(5).Components.Values
                        subst.PartialVolume = 1 / 1000 * subst.ConstantProperties.Molar_Weight / Me.m_props.liq_dens_rackett(T, subst.ConstantProperties.Critical_Temperature, subst.ConstantProperties.Critical_Pressure, subst.ConstantProperties.Acentric_Factor, subst.ConstantProperties.Molar_Weight, subst.ConstantProperties.Z_Rackett, P, Me.AUX_PVAPi(subst.Name, T))
                    Next
                Case Phase.Vapor
                    Dim partvol As New Object
                    Dim i As Integer = 0
                    partvol = Me.m_pr.CalcPartialVolume(T, P, RET_VMOL(phase), RET_VKij(), RET_VTC(), RET_VPC(), RET_VW(), RET_VTB(), "V", 0.0001)
                    i = 0
                    For Each subst As Substance In Me.CurrentMaterialStream.Phases(2).Components.Values
                        subst.PartialVolume = partvol(i)
                        i += 1
                    Next
            End Select
        End Sub

        Public Overrides Function AUX_VAPDENS(ByVal T As Double, ByVal P As Double) As Double
            Dim val As Double
            Dim Z As Double = Me.m_lk.Z_LK("V", T / Me.AUX_TCM(Phase.Vapor), P / Me.AUX_PCM(Phase.Vapor), Me.AUX_WM(Phase.Vapor))(0)
            val = P / (Z * 8.314 * T) / 1000 * AUX_MMM(Phase.Vapor)
            Return val
        End Function

        Public Overrides Function DW_CalcEntropy(ByVal Vx As Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double

            Dim S As Double

            If st = State.Liquid Then
                Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                    Case 0 'LK
                        S = Me.m_lk.S_LK_MIX("L", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Vx))
                    Case 1 'Ideal
                        S = Me.RET_Hid_L(298.15, T, Vx) / T - Me.RET_HVAPM(Me.AUX_CONVERT_MOL_TO_MASS(Vx), T) / T
                    Case 2 'Excess
                        S = (Me.RET_Hid_L(298.15, T, Vx) + Me.m_uni.HEX_MIX(T, Vx, Me.RET_VQ, Me.RET_VR, Me.RET_VEKI) / Me.AUX_MMM(Vx)) / T - Me.RET_HVAPM(Me.AUX_CONVERT_MOL_TO_MASS(Vx), T) / T
                End Select
            Else
                S = Me.m_lk.S_LK_MIX("V", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, Me.RET_Sid(298.15, T, P, Vx))
            End If

            Return S

        End Function

        Public Overrides Function DW_CalcEntropyDeparture(ByVal Vx As Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double
            Dim S As Double

            If st = State.Liquid Then
                Select Case Me.Parameters("PP_ENTH_CP_CALC_METHOD")
                    Case 0 'LK
                        S = Me.m_lk.S_LK_MIX("L", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, 0)
                    Case 1 'Ideal
                        S = -Me.RET_HVAPM(Me.AUX_CONVERT_MOL_TO_MASS(Vx), T) / T
                    Case 2 'Excess
                        S = (Me.m_uni.HEX_MIX(T, Vx, Me.RET_VQ, Me.RET_VR, Me.RET_VEKI) / Me.AUX_MMM(Vx)) / T - Me.RET_HVAPM(Me.AUX_CONVERT_MOL_TO_MASS(Vx), T) / T
                End Select
            Else
                S = Me.m_lk.S_LK_MIX("V", T, P, Vx, RET_VKij(), RET_VTC, RET_VPC, RET_VW, RET_VMM, 0)
            End If

            Return S

        End Function

        Public Overrides Function DW_CalcFugCoeff(ByVal Vx As Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double()

            App.WriteToConsole(Me.ComponentName & " fugacity coefficient calculation for phase '" & st.ToString & "' requested at T = " & T & " K and P = " & P & " Pa.", 2)
            App.WriteToConsole("Compounds: " & Me.RET_VNAMES.ToArrayString, 2)
            App.WriteToConsole("Mole fractions: " & Vx.ToArrayString(), 2)

            Dim prn As New ThermoPlugs.PR

            Dim n As Integer = Vx.Length - 1
            Dim lnfug(n), ativ(n) As Double
            Dim fugcoeff(n) As Double
            Dim i As Integer

            Dim Tc As Object = Me.RET_VTC()

            If st = State.Liquid Then
                ativ = Me.m_uni.GAMMA_MR(T, Vx, Me.RET_VQ, Me.RET_VR, Me.RET_VEKI)
                For i = 0 To n
                    If T / Tc(i) >= 1 Then
                        lnfug(i) = Math.Log(AUX_KHenry(Me.RET_VNAMES(i), T) / P)
                    Else
                        lnfug(i) = Math.Log(ativ(i) * Me.AUX_PVAPi(i, T) / P)
                    End If
                Next
            Else
                If Not Me.Parameters.ContainsKey("PP_IDEAL_VAPOR_PHASE_FUG") Then Me.Parameters.Add("PP_IDEAL_VAPOR_PHASE_FUG", 0)
                If st = State.Vapor And Me.Parameters("PP_IDEAL_VAPOR_PHASE_FUG") = 1 Then
                    For i = 0 To n
                        lnfug(i) = 0.0#
                    Next
                Else
                    lnfug = prn.CalcLnFug(T, P, Vx, Me.RET_VKij, Me.RET_VTC, Me.RET_VPC, Me.RET_VW, Nothing, "V")
                End If
            End If

            For i = 0 To n
                fugcoeff(i) = Exp(lnfug(i))
            Next

            App.WriteToConsole("Result: " & fugcoeff.ToArrayString(), 2)

            Return fugcoeff

        End Function

    End Class

End Namespace


